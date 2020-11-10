using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Web;
using WebApplication2.Controllers;

namespace WebApplication2.Tasks
{

    /// <summary>
    /// Recurring task for archiving uploaded files. Files with zip extension will be skipped.
    /// </summary>
    public class ZipProcessingTask : IDisposable
    {

        /// <summary>
        /// Stores timer that will be used to poll the zip archiving operation.
        /// This instance will also be used as a multithreading padlock.
        /// </summary>
        private System.Timers.Timer _timer = new System.Timers.Timer();

        /// <summary>
        /// Gets or sets (private) if task is currently running.
        /// </summary>
        public bool IsRunningTask { get; private set; }

        /// <summary>
        /// Gets or sets (private) when task was last time run.
        /// </summary>
        public DateTime LastRun { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ZipProcessingTask()
        {
            this._timer.Interval = 1;
            this._timer.Elapsed += _timer_Elapsed;
            this.LastRun = DateTime.Now;
        }

        /// <summary>
        /// Timer elapsed handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            // We use multithreading lock as a transaction because we are reading two variables
            //which might change values when code executes from one instruction to another.
            lock (this._timer)
            {
                // Run task every one second.
                if (DateTime.Now - LastRun < TimeSpan.FromSeconds(1))
                    return;
                // Check and set that task is running.
                if (this.IsRunningTask)
                    return;
                this.IsRunningTask = true;
            }
            try
            {
                // Get first file from upload folder that is not a zip file.
                var first = Directory.GetFiles(FilesController.UploadDir, "!*.zip")?.FirstOrDefault();
                // If none, return cause we have nothing to process for now.
                if (string.IsNullOrWhiteSpace(first))
                    return;
                // Get file info.
                var fi = new FileInfo(first);
                // Open file where we are going to write the archive.
                using (var ofs = File.Open($"{first}.zip", FileMode.Create, FileAccess.Write))
                using (var archive = new ZipArchive(ofs, ZipArchiveMode.Create))
                {
                    // Create zip file entry in the zip file.
                    var zipFileEntry = archive.CreateEntry(fi.Name);
                    // Open both zip file entry stream and input file stream (the one uploaded).
                    using (var zfs = zipFileEntry.Open())
                    using (var ifs = File.Open(first, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        // Copy uploaded file to zip file entry stream.
                        ifs.CopyTo(zfs);
                    }
                }
                // Delete file.
                File.Delete(first);
            }
            catch (Exception exc)
            {
                // Trace whatever error we have.
                System.Diagnostics.Trace.TraceError($"Task error: {exc}");
            }
            finally
            {
                // We use multithreading lock as a transaction because we are setting two variables
                //which might change values when code executes from one instruction to another.
                lock (this._timer)
                {
                    IsRunningTask = false;
                    LastRun = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// Disposes gracefully.
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            // Suppress finalization.
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose helper method.
        /// </summary>
        /// <param name="disposing">True if outside destructor, false otherwise.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && _timer != null)
            {
                _timer.Dispose();
                _timer = null;
            }
        }

        /// <summary>
        /// Instance finalizer (aka destructor).
        /// </summary>
        ~ZipProcessingTask()
        {
            Dispose(false);
        }

    }

}