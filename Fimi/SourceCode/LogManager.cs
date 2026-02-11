namespace Fimi
{
    public class LogManager
    {
        private readonly string _path;

        public LogManager(string path)
        {
            if (Directory.Exists(path) == false)
            {
                Directory.CreateDirectory(path);
            }

            _path = path;
        }

        public void Error(string error)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(_path, DateTime.Now.ToString("dd-MM-yyyy") + ".log.xml"), true))
            {
                sw.WriteLine("\n" + DateTime.Now.ToString("dd:MM:yyyy-HH:mm:ss - ") + error);
            }
        }

        public void Error(Exception error)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(_path, DateTime.Now.ToString("dd-MM-yyyy") + ".log.xml"), true))
            {
                sw.WriteLine("\n" + DateTime.Now.ToString("dd:MM:yyyy-HH:mm:ss - ") + error.ToString());
            }
        }

        public void Info(string info)
        {
            using (StreamWriter sw = new StreamWriter(Path.Combine(_path, DateTime.Now.ToString("dd-MM-yyyy") + ".log.xml"), true))
            {
                sw.WriteLine("\n" + DateTime.Now.ToString("dd:MM:yyyy-HH:mm:ss - ") + info);
            }
        }
    }
}
