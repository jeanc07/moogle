public class computeScores
{
    /// <summary>
    /// Método para calcular los scores de todos los documentos en dependecia del query.
    /// </summary>
    /// <param name="sourcepath"></Directorio>
    /// <param name="query"></Consulta>
    /// <param name="termsquery"></param>
    /// <param name="dictDistancia"></Diccionario que se usara en caso de uso del operador de distancia(~)>
    /// <param name="dictPrioridad"></Diccionario que se usara en caso de uso del operador de prioridad (*)>
    /// <param name="dictTempsnippet"></Diccionario donde se guardara el texto que se mostrará en pantalla por documento>
    /// <param name="dictTF_IDF"></Diccionario donde se guardara el TF*IDF de cada documento>
    /// <returns></El score de cada documento según la consulta>
    public static double[] computeScore(string sourcepath, string query, List<string> termsquery, Dictionary<string, string> dictDistancia, 
                                            Dictionary<string, int> dictPrioridad, Dictionary<string , List<string>> dictTempsnippet,
                                            Dictionary<string, Dictionary<string, double>> dictTF_IDF)
        {
            var arrayDirectory = Directory.EnumerateFiles(sourcepath);   
            Dictionary<string, Dictionary<string, int>> dictFrecuencia = new Dictionary<string, Dictionary<string, int>>();  
            double [] score = new double [arrayDirectory.Count()];          
            List<string> temp = new List<string>();   
            List<int> fraseFrecuencia = new List<int>();   

            for (int i = 0; i < termsquery.Count; i++)
            {
                if (termsquery[i] == "" && termsquery.Count > 1)
                    i++;
                if(termsquery[i][0] == '*' || termsquery[i][0] == '!' || termsquery[i][0] == '^')
                    fraseFrecuencia.Add(Countdocs.numerosDocs(termsquery[i].Substring(1,termsquery[i].Length-1).ToLower(), sourcepath)); 
                else
                    fraseFrecuencia.Add(Countdocs.numerosDocs(termsquery[i].ToLower(), sourcepath));
            }

            int count = arrayDirectory.Count();
            for(int j = 0; j < count; j++)
            {
                string filecontent = File.ReadAllText(arrayDirectory.ElementAt(j));        
                Dictionary<string, int> dictTemp = new Dictionary<string, int>();  
                Dictionary<string, double> dictTempD = new Dictionary<string, double>();  
                List<string> tFrases = new List<string>();
                string [] words = filecontent.Split(new char[] {' ','(',')',',',';','"','.','!','?','-','\0','\n'});

                for (int i = 0; i < termsquery.Count; i++)
                {  
                    if(fraseFrecuencia[i] > 0 && termsquery[i] != "")  
                    {         
                        if(termsquery[i][0] == '^' || termsquery[i][0] == '!')
                            temp = filecontent.ToLower().Split((" "+termsquery[i].Substring(1,termsquery[i].Length-1)+" ").ToLower()).ToList<string>();
                        else
                            temp = filecontent.ToLower().Split((" "+termsquery[i]+" ").ToLower()).ToList<string>();    
                        
                        dictTemp.Add(termsquery[i], temp.Count - 1);  

                        if (temp.Count == 1 && termsquery[i][0] == '^')
                        {
                            dictTempD.Add(termsquery[i], 0); 
                        }
                        else if (temp.Count > 1)
                        {
                            if(termsquery[i][0] == '!')
                            {
                                dictTempD.Add(termsquery[i], 0); 
                            }
                            else
                            {
                                List<string> phrases = filecontent.ToLower().Split(new char[] {'(',')',',',';','"','.','!','?','-', ' ','*','\0','\n'}).Distinct().Where(d => !ComputeSnippets.SplitList(d)).ToList<string>();
                                int distanciaTotal = 0;

                                if(termsquery.Contains("~") && dictDistancia.Count > 1)
                                    distanciaTotal = computeDistanciaTotal(dictDistancia, filecontent, termsquery[i]);

                                if(distanciaTotal > 0)
                                {

                                    dictTempD.Add(termsquery[i], (1 - ((distanciaTotal*100)/(double)phrases.Count)/100) + 
                                        (dictPrioridad.ElementAt(i).Value * (((temp.Count - 1)/(double)words.Length)*
                                            (Math.Log10(count/(double)fraseFrecuencia[i]+1)))));    
                                }
                                else
                                {                                    
                                    dictTempD.Add(termsquery[i], (dictPrioridad.ElementAt(i).Value * (((temp.Count - 1)/
                                    (double)words.Length)*(Math.Log10(count/(double)fraseFrecuencia[i]+1)))));
                                }

                                if(termsquery[i][0] == '*' || termsquery[i][0] == '^')
                                    termsquery[i] = termsquery[i].Substring(1,termsquery[i].Length-1);
                                if(distanciaTotal  == 1)
                                {
                                    string querytemp = ComputeSnippets.computeUnion(termsquery);
                                    temp = filecontent.ToLower().Split((" "+querytemp+" ").ToLower()).ToList<string>();
                                    tFrases.Add(ComputeSnippets.ComputeSnippet(temp, querytemp));
                                }
                                else 
                                    tFrases.Add(ComputeSnippets.ComputeSnippet(temp,termsquery[i]));                       
                            }
                        }                      
                    }
                }
                dictTempsnippet.Add(Path.GetFileName(arrayDirectory.ElementAt(j)), tFrases); 
                dictFrecuencia.Add(Path.GetFileName(arrayDirectory.ElementAt(j)), dictTemp);  
                dictTF_IDF.Add(Path.GetFileName(arrayDirectory.ElementAt(j)), dictTempD);     

                if (filecontent.ToLower().Contains(query.ToLower()))
                {  
                    if(dictTempD.Count() > 0)
                        score[j] = dictTempD.Values.Max()*dictTempD.Count();
                    else          
                        score[j] = 0;
                }
            }         


            return score;
        }

    /// <summary>
    /// Método para calcular el score de cada documento si no se encuentra nada exactamente igual
    /// a lo que pide el usuario en la consulta. En este caso calcularía el score según lo devuelto
    /// por el algoritmo Damareau-Levenshtein ya sea la frase o palabra más parecida.
    /// </summary>
    /// <param name="sourcepath"></Directorio>
    /// <param name="query"></Consulta>
    /// <param name="termsquery"></param>
    /// <param name="dictDistancia"></Diccionario que se usara en caso de uso del operador de distancia (~)>
    /// <param name="dictPrioridad"></Diccionario que se usara en caso de uso del operador de prioridad (*)>
    /// <param name="dictTempsnippet"></Diccionario donde se guardara el texto que se mostrará en pantalla por documento>
    /// <param name="dictTF_IDF"></Diccionario donde se guardara el TF*IDF de cada documento>
    /// <returns></Retorna el valor del TF*IDF de cada documento con lo devuelto por Damareau-Levenshtein>
    public static double[] computeScoreLevensh(string sourcepath, string query, List<string> termsquery, Dictionary<string, string> dictDistancia, 
                                    Dictionary<string, int> dictPrioridad, Dictionary<string , List<string>> dictTempsnippet,
                                    Dictionary<string, Dictionary<string, double>> dictTF_IDF)
    {
        var arrayDirectory = Directory.EnumerateFiles(sourcepath);   //Directorio donde estan los archivos de texto
        Dictionary<string, Dictionary<string, int>> dictFrecuencia = new Dictionary<string, Dictionary<string, int>>();  
        double [] score = new double [arrayDirectory.Count()];          
        List<string> temp = new List<string>();   // Texto sin el query y sin palabras parecidas
        List<int> fraseFrecuencia = new List<int>();   // Cantidad de documentos en los que esta el query
        dictTempsnippet.Clear();
        dictTF_IDF.Clear();
        dictFrecuencia.Clear();

        int count = arrayDirectory.Count();
        for(int j = 0; j < count; j++)
        {
            string filecontent = File.ReadAllText(arrayDirectory.ElementAt(j));        
            Dictionary<string, int> dictTemp = new Dictionary<string, int>();  
            Dictionary<string, double> dictTempD = new Dictionary<string, double>();  
            List<string> tFrases = new List<string> (); 
            //fraseFrecuencia.Clear();
            string [] words = filecontent.Split(new char[] {' ','(',')',',',';','"','.','!','?','-'});


            for (int i = 0; i < termsquery.Count; i++)
            {
                if(termsquery[i] != "")
                {
                    if (termsquery[i][0] == '^' && temp.Count() == 1)
                    {
                        dictTempD.Add(termsquery[i], 0);
                    }
                    if (termsquery[i][0] != '^')
                    {
                        string[] totalPhrases = filecontent.ToLower().Split(new char[] { '(', ')', ',', ';', '"', '.', '!', '?', '-', ' ' })
                        .Distinct().Where(d => !ComputeSnippets.SplitList(d)).ToArray<string>();
                        string[] phrases = filecontent.ToLower().Split(new char[] { '(', ')', ',', ';', '"', '.', '!', '?', '-', ' ' })
                        .Distinct().Where(d => !ComputeSnippets.SplitList(d) && Levenshtein.Calculatequery(termsquery[i].ToLower(), d)).ToArray<string>();

                        if (termsquery[i][0] == '^' || termsquery[i][0] == '!' || termsquery[i][0] == '*')
                        {
                            phrases = filecontent.ToLower().Split(new char[] { '(', ')', ',', ';', '"', '.', '!', '?', '-', ' ' })
                            .Distinct().Where(d => !ComputeSnippets.SplitList(d) && Levenshtein.Calculatequery(termsquery[i].Substring(1, termsquery[i].Length - 1).ToLower(), d)).ToArray<string>();
                            if (phrases.Count() > 0)
                                temp = filecontent.ToLower().Split(" " + phrases[0] + " ").ToList<string>();
                        }
                        if (phrases.Count() > 0)
                        {
                            int index = 0;
                            fraseFrecuencia.Add(Countdocs.numerosDocs(phrases[0], sourcepath));
                            if (fraseFrecuencia[i] == 0)
                            {
                                index = i + 1;
                            }
                            if (termsquery[i][0] == '!' && temp.Count() > 1)
                            {
                                dictTempD.Add(termsquery[i], 0);
                            }
                            else if (termsquery[i][0] == '^' && temp.Count() == 1)
                            {
                                dictTempD.Add(termsquery[i], 0);
                            }
                            else
                            {
                                temp = filecontent.ToLower().Split(" " + phrases[0] + " ").ToList<string>();
                                dictTemp.Add(termsquery[i], temp.Count() - 1);
                                dictTempD.Add(termsquery[i], ((temp.Count - 1) / ((double)totalPhrases.Length)) *
                                (Math.Log10(count / (double)fraseFrecuencia[index] + 1)));
                                tFrases.Add(ComputeSnippets.ComputeSnippet(temp, phrases[0]));
                            }

                        }
                    }
                    else
                    {
                        dictTempD.Add(termsquery[i], 0);
                    }
                }
            }
                
            dictTempsnippet.Add(Path.GetFileName(arrayDirectory.ElementAt(j)), tFrases);  
            dictFrecuencia.Add(Path.GetFileName(arrayDirectory.ElementAt(j)), dictTemp);  
            dictTF_IDF.Add(Path.GetFileName(arrayDirectory.ElementAt(j)), dictTempD);     

            if (filecontent.ToLower().Contains(query.ToLower()))
            {  
                if(dictTempD.Count() > 0)
                    score[j] = dictTempD.Values.Max()*dictTempD.Count();
                else          
                    score[j] = 0;
            }
        }            
        return score;
    }

    /// <summary>
    /// Método que realiza los cálculos correspondientes para el uso del operador de distancia (~)
    /// </summary>
    /// <param name="dictDistancia"></Diccionario que contiene la distancia que existe entre dos string>
    /// <param name="content"></string a encontrar>
    /// <param name="termsquery"></string a encontrar>
    /// <returns></La distancia que existe entre dos términos>
    public static int computeDistanciaTotal(Dictionary<string, string> dictDistancia, string content, string termsquery)
    {
        int distanciaTotal = 999999;

        string? val = " ";
        if(dictDistancia.TryGetValue(termsquery, out val))
        {
            List<string> phrases1 = content.ToLower().Split(termsquery).ToList<string>();

            List<string> phrases2 = content.ToLower().Split(val).ToList<string>();

            int lontigud = phrases1.Count;

            if(phrases2.Count < phrases1.Count)
                lontigud = phrases2.Count;

            for (int i = 0; i < lontigud; i++)
            {
                int l1 = phrases1[i].ToLower().Split(new char[] {'(',')',',',';','"','.','!', '¿','?','-', ' ','*','\0','\n'}).ToList<string>().Count;
                int l2 = phrases2[i].ToLower().Split(new char[] {'(',')',',',';','"','.','!', '¿','?','-', ' ','*','\0','\n'}).ToList<string>().Count;;
                if(Math.Abs(l2 - l1) < distanciaTotal)
                    distanciaTotal = Math.Abs(l2 - l1);

                if(distanciaTotal == 0)
                    l1 =9;
            }
        }        

        return distanciaTotal;
    }
}
