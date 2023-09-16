public class ComputeSnippets
{
    /// <summary>
    /// Método para armar la frase que se devolvera en los items.
    /// </summary>
    /// <param name="temp"></Texto del documento>
    /// <param name="query"></Consulta>
    /// <returns></Retorna un fragmento del documento que contiene la consulta de aproximadamente 50 palabras>
    public static string ComputeSnippet(List<string> temp, string query)
    {
        string result = "";
        string querytemp = query;
        string textafterquery = querytemp;
        string textbeforequery = "";
        int count = 0;
        if (temp.Count > 1)
        {
            string[] phrases1 = temp[0].Split(new char[] {' '});
            string[] phrases2 = temp[1].Split(new char[] {' '});
            if(phrases1.Length > 25 && phrases2.Length > 25) 
            {
                for (int i = 0; i < 25; i++)
                {
                    textafterquery+= " " + phrases2[i];
                }
                for (int i = phrases1.Length - 1; i >= 0 && count != 25; i--)
                {
                    textbeforequery = phrases1[i] + " " + textbeforequery;
                    count++;
                }
            }else if(phrases1.Length < 25 && phrases2.Length > 25)
            {
                for (int i = 0; i < 25; i++)
                {
                    textafterquery+= " " + phrases2[i];
                }
                for (int i = phrases1.Length - 1; i >= 0; i--)
                {
                    textbeforequery = phrases1[i] + " " + textbeforequery;
                }
            }else if(phrases1.Length > 25 && phrases2.Length < 25)
            {
                for (int i = 0; i < phrases2.Length-1; i++)
                {
                    textafterquery+= " " + phrases2[i];
                }
                for (int i = phrases1.Length - 1; i >= 0 && count != 25; i--)
                {
                    textbeforequery = phrases1[i] + " " + textbeforequery;
                    count++;
                }
            }
            else
            {
                for (int i = 0; i < phrases2.Length-1; i++)
                {
                    textafterquery+= " " + phrases2[i];
                }
                for (int i = phrases1.Length - 1; i >= 0; i--)
                {
                    textbeforequery = phrases1[i] + " " + textbeforequery;
                }
            }
            result = textbeforequery + textafterquery;
        }
        return result;
    }

    /// <summary>
    /// Método forma la frase cuando se usan los operadores
    /// </summary>
    /// <param name="termsquery"></Lista que contiene la frase sobre la cual se usaron los operadores>
    /// <returns></Retorna la frase quitando los operadores>
    public static string computeUnion(List<string> termsquery)
    {
        string terms = "";
        string [] symbols = new string [] {"!", "~", "^", "*"};

        for (int i = 0; i < termsquery.Count - 1; i++)
        {
            if(!symbols.Contains(termsquery[i]))
                terms+=(termsquery[i] + " ");   
        }

        if(!symbols.Contains(termsquery[termsquery.Count - 1]))
            terms+=(termsquery[termsquery.Count - 1]);         

        return terms;
    }

    /// <summary>
    /// Método que se usa para elminar palabras del vocabulario que no tienen relevancia
    /// </summary>
    /// <param name="filecontent"></Palabra a analizar>
    /// <returns></Retorna true si filecontent es una de las palabras contenidas en el array de string tokens y falso si no>
    public static bool SplitList(string filecontent)  
    {
        bool result = false;
        string[] tokens = {"a","ante","con","contra","de","desde","en","entre",
        "para","por","según","sin","sobre","tras","y","e","ni","que","o","u","yo",
        "tu","el","ella","nosotros","ustedes","vosotros","ellos","ellas","la","las","lo","los","uno",
        "una","unos","unas","es","un","su","se"};
        string[] filecontentemp = filecontent.Split(new char[] {' '});
        for (int i = 0; i < tokens.Length; i++)
        {
            if (filecontentemp.Contains(tokens[i]))
            {
                result = true;
            }
        }
        
        return result;
    }
}