namespace XSharpCreateInstanceChecker
{
    public class FileWriter
    {
        private readonly string filePath;

        public FileWriter(string filePath)
        {
            this.filePath = filePath;
        }

        public void Write(string text)
        {
            File.WriteAllText(filePath, text);

        }




    }
}
