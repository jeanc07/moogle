public class Countdocs
{
    /// <summary>
    /// Método para contar en cuantos documentos aparece un término.
    /// </summary>
    /// <param name="word"></Término>
    /// <param name="sourcepath"></Directorio>
    /// <returns></Retorna la cantidad de documentos que contienen el término>
    public static int numerosDocs(string word, string sourcepath)
    {
        int count = 0;
        var arrayDirectory = Directory.EnumerateFiles(sourcepath);
        foreach(var file in arrayDirectory)
        {
            string filecontent = File.ReadAllText(file);
            if (filecontent.ToLower().Contains(word))
            {      
                count++;
            }
        }

        return count;
    }
}


