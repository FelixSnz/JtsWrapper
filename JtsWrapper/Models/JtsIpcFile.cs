using NLog;
using System;
using System.IO;
using System.Linq;


namespace JtsWrapper.Models
{

    /// <summary>
    /// Johnson Tracking System inter-process communication File class
    /// </summary>
    public class JtsIpcFile  
    {
        private string basePath = AppDomain.CurrentDomain.BaseDirectory;
        private string FilePath;
        public static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="JtsIpcFile"/> class.
        /// </summary>
        /// <param name="FileName">The name of the IPC file.</param>
        public JtsIpcFile(string FileName)
        {
            FilePath = Path.Combine(basePath, FileName);
        }

        /// <summary>
        /// Reads the contents of the IPC file.
        /// </summary>
        /// <returns>Returns the contents of the IPC file as a string, or null if an error occurs.</returns>
        public string Read()
        {
            try
            {
                Logger.Info($"Reading from '{FilePath}' ");
                if (!File.Exists(FilePath))
                {
                    Logger.Warn($"'{Path.GetFileName(FilePath)}' file not found");
                    return null;
                }
                else
                {
                    var lines = File.ReadAllLines(FilePath);

                    if (lines.Length == 1)
                    {
                        string contents = lines.First();
                        Logger.Info($"Successfully read: '{contents}'");
                        return contents;
                    }
                    else
                    {
                        string linesText = string.Join(", ", lines);
                        Logger.Warn($"Found more than two lines ({linesText})");
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return null;
            }
        }

        /// <summary>
        /// Writes the specified contents to the IPC file, overwriting any existing content.
        /// </summary>
        /// <param name="contents">The contents to write to the IPC file.</param>
        public void Write(string contents)
        {
            try
            {
                Logger.Info($"Writting to '{FilePath}'");
                if (!File.Exists(FilePath))
                {
                    Logger.Warn($"'{Path.GetFileName(FilePath)}' file not found");
                    Logger.Info($"creating '{Path.GetFileName(FilePath)}' file...");
                    File.WriteAllText(FilePath, contents);
                }
                else
                {
                    File.WriteAllText(FilePath, contents);
                }
                Logger.Info($"Successfully Written!");
            }
            catch (Exception ex)
            {

                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Deletes the IPC file if it exists.
        /// </summary>
        public void Delete()
        {
            try
            {
                Logger.Info($"Deleting '{FilePath}' file...");
                if (!File.Exists(FilePath))
                {
                    Logger.Warn($"'{Path.GetFileName(FilePath)}' file not found");
                }
                else
                {
                    File.Delete(FilePath);
                }
                Logger.Info($"Successfully deleted!");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        /// <summary>
        /// Clears the contents of the IPC file.
        /// </summary>
        public void Clear()
        {
            try
            {
                Logger.Info($"Clearing contents of '{FilePath}' file...");
                {
                    Logger.Warn($"'{Path.GetFileName(FilePath)}' file not found");
                }
                File.WriteAllText(FilePath, string.Empty);
                Logger.Info($"Successfully cleared!");
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }
    }
}
