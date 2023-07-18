namespace MoogleEngine;
using System;
using System.Collections.Generic;

public class DocsFinal: IEquatable<DocsFinal>, IComparable<DocsFinal>
{
    public string? filename;
    public string? snippet;
    public float score;
    public List<string>? Frases { get; set;}

    public override string ToString() {return filename + "-" + snippet + "-" + score;}

    public override bool Equals (object ?obj) 
    {
        if(obj == null) return false;
        DocsFinal ?d = obj as DocsFinal;
        if(d == null) return false;
        else return Equals(d);
    }

    public int CompareTo(DocsFinal ?d)
    {
        if(d == null) return 1;
        else return this.score.CompareTo(d.score);
    }

    public bool Equals(DocsFinal ?d)
    {
        if(d == null) return false;
        return this.score.Equals(d.score);
    }
};
public static class Moogle
{
    private static string suggestion;

    private static double minDistance;

    public static SearchResult Query(string query) 
    {
        suggestion = "";
        minDistance = 9999.0;
        string sourcepath = @"..\Content";
        var arrayDirectory = Directory.EnumerateFiles(sourcepath);
        Dictionary<string, Dictionary<string, double>> dictTF_IDF = new Dictionary<string, Dictionary<string, double>>();
        Dictionary<string, int> dictPrioridad = new Dictionary<string, int>();  
        Dictionary<string, string> dictDistancia = new Dictionary<string, string>();
        List<string> termsquery = new List<string>();
        List<DocsFinal> docs = new List<DocsFinal>();
        Dictionary<string , List<string>> dictTempsnippet = new Dictionary<string, List<string>>();
        double [] score = new double [arrayDirectory.Count()];
        string tempquery = "";

        List<string> tmpPrioridad = new List<string>();  
        List<string> termsDistancia = new List<string>(); 
        query = query.Trim();
        tempquery = BuildQuery(query);
        tempquery = tempquery.Trim();
        termsquery = tempquery.Split(new char[] {'(',')',',',';','"','.','?','-', ' ','\0','\n'})
        .Distinct().Where( d => !SplitList(d)).ToList<string>();
        for (int i = 0; i < termsquery.Count; i++)
        {
            tmpPrioridad = termsquery[i].Split(new char[] {'*'}).ToList<string>(); 
            if(tmpPrioridad.Count > 1)
                dictPrioridad.Add(termsquery[i], 100);
            else
                dictPrioridad.Add(termsquery[i], 1);   

            termsquery[i] = tmpPrioridad[tmpPrioridad.Count-1].ToLower();
        }

        termsDistancia = query.Split(new char[] {'~'}).ToList<string>();
        List<string> tmp1 = new List<string>();
        List<string> tmp2 = new List<string>();
        if(termsDistancia.Count > 1 && !(termsDistancia[0] == "" || termsDistancia[1] == ""))
        {
            for (int i = 0; i < termsDistancia.Count - 1; i++)
            {
                tmp1 = termsDistancia[i].Split(new char[] {'(',')',',',';','"','.','?','-', ' ','\0','\n'})
                .Distinct().Where( d => !SplitList(d)).ToList<string>();
                tmp2 = termsDistancia[i+1].Split(new char[] {'(',')',',',';','"','.','?','-', ' ','\0','\n'})
                .Distinct().Where(d => !SplitList(d)).ToList<string>();
                dictDistancia.Add(tmp1[0].ToLower(), tmp2[tmp2.Count-1].ToLower());
                dictDistancia.Add(tmp2[tmp2.Count-1].ToLower(), tmp1[0].ToLower());
            }
        }
        else
        {
            termsDistancia.Remove("");
        }

        score = computeScore(sourcepath, query, termsquery, dictDistancia, dictPrioridad, dictTempsnippet, dictTF_IDF);
        docs = doRanking(dictTF_IDF, score, dictTempsnippet);

        SearchItem[] items = new SearchItem[docs.Count()];

        for(int i = docs.Count-1; i >= 0; i--) 
        {
            if(docs[i].Frases.Count() > 0)
                items[i] = new SearchItem(docs[i].filename ,docs[i].Frases[0], docs[i].score);
            else 
                items[i] = new SearchItem(docs[i].filename ,"", docs[i].score);

        }
        if (docs.Count() == 0)
        {
            score = computeScoreLevensh(sourcepath, query, termsquery, dictDistancia, dictPrioridad, dictTempsnippet, dictTF_IDF);
            docs = doRanking(dictTF_IDF, score, dictTempsnippet);
            SearchItem[] items2 = new SearchItem[docs.Count()];
            for(int i = docs.Count-1; i >= 0; i--) 
            {
                if(docs[i].Frases.Count() > 0)
                    items2[i] = new SearchItem(docs[i].filename ,docs[i].Frases[0], docs[i].score);
                else 
                    items2[i] = new SearchItem(docs[i].filename ,"", docs[i].score);

            }
            items = items2;
        }

        if (docs.Count() <= 0)
        {
            SearchItem[] items1 = new SearchItem[1];
            items1[0] = new SearchItem(" ","Su busqueda no tiene relevancia. Pruebe con otra busqueda",0);
            items = items1;
        }
        return new SearchResult(items , suggestion);
    }
    
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

    public static int DamerauLevenshteinDistance(string string1, string string2, int threshold)
    {
        if (string1.Equals(string2))
            return 0;

        if (string.IsNullOrEmpty(string1) || string.IsNullOrEmpty(string2))
            return (string1 ?? "").Length + (string2 ?? "").Length;


        if (string1.Length > string2.Length)
        {
            var tmp = string1;
            string1 = string2;
            string2 = tmp;
        }

        if (string2.Contains(string1))
            return string2.Length - string1.Length;

        var length1 = string1.Length;
        var length2 = string2.Length;

        var d = new int[length1 + 1, length2 + 1];

        for (var i = 0; i <= d.GetUpperBound(0); i++)
            d[i, 0] = i;

        for (var i = 0; i <= d.GetUpperBound(1); i++)
            d[0, i] = i;

        for (var i = 1; i <= d.GetUpperBound(0); i++)
        {
            var im1 = i - 1;
            var im2 = i - 2;
            var minDistance = threshold;

            for (var j = 1; j <= d.GetUpperBound(1); j++)
            {
                var jm1 = j - 1;
                var jm2 = j - 2;
                var cost = string1[im1] == string2[jm1] ? 0 : 1;

                var del = d[im1, j] + 1;
                var ins = d[i, jm1] + 1;
                var sub = d[im1, jm1] + cost;

                d[i, j] = del <= ins && del <= sub ? del : ins <= sub ? ins : sub;

                if (i > 1 && j > 1 && string1[im1] == string2[jm2] && string1[im2] == string2[jm1])
                    d[i, j] = Math.Min(d[i, j], d[im2, jm2] + cost);

                if (d[i, j] < minDistance)
                    minDistance = d[i, j];
            }

            if (minDistance > threshold)
                return int.MaxValue;
        }

        return d[d.GetUpperBound(0), d.GetUpperBound(1)] > threshold 
            ? int.MaxValue 
            : d[d.GetUpperBound(0), d.GetUpperBound(1)];
    }
    
    public static bool Calculatequery(string query, string frase)
    {
        double difference = Math.Abs(frase.Length - query.Length);
        double val = DamerauLevenshteinDistance(query , frase , 3) ;

        if(frase.Length >= query.Length/2)
        {
            if(val <= difference + 0.25)
            {
                if(val < minDistance)
                {
                    minDistance = val;
                    suggestion = frase;
                }
                    
                return true;
            }
        }
        return false;
    }

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
                fraseFrecuencia.Add(numerosDocs(termsquery[i].Substring(1,termsquery[i].Length-1).ToLower(), sourcepath)); 
            else
                fraseFrecuencia.Add(numerosDocs(termsquery[i].ToLower(), sourcepath));
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
                            List<string> phrases = filecontent.ToLower().Split(new char[] {'(',')',',',';','"','.','!','?','-', ' ','*','\0','\n'}).Distinct().Where(d => !SplitList(d)).ToList<string>();
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
                                string querytemp = computeUnion(termsquery);
                                temp = filecontent.ToLower().Split((" "+querytemp+" ").ToLower()).ToList<string>();
                                tFrases.Add(ComputeSnippet(temp, querytemp));
                            }
                            else 
                                tFrases.Add(ComputeSnippet(temp,termsquery[i]));                       
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

    static string computeUnion(List<string> termsquery)
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
                        .Distinct().Where(d => !SplitList(d)).ToArray<string>();
                        string[] phrases = filecontent.ToLower().Split(new char[] { '(', ')', ',', ';', '"', '.', '!', '?', '-', ' ' })
                        .Distinct().Where(d => !SplitList(d) && Calculatequery(termsquery[i].ToLower(), d)).ToArray<string>();

                        if (termsquery[i][0] == '^' || termsquery[i][0] == '!' || termsquery[i][0] == '*')
                        {
                            phrases = filecontent.ToLower().Split(new char[] { '(', ')', ',', ';', '"', '.', '!', '?', '-', ' ' })
                            .Distinct().Where(d => !SplitList(d) && Calculatequery(termsquery[i].Substring(1, termsquery[i].Length - 1).ToLower(), d)).ToArray<string>();
                            if (phrases.Count() > 0)
                                temp = filecontent.ToLower().Split(" " + phrases[0] + " ").ToList<string>();
                        }
                        if (phrases.Count() > 0)
                        {
                            int index = 0;
                            fraseFrecuencia.Add(numerosDocs(phrases[0], sourcepath));
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
                                tFrases.Add(ComputeSnippet(temp, phrases[0]));
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

    static string ComputeSnippet(List<string> temp, string query)
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

    public static List<DocsFinal> doRanking(Dictionary<string, Dictionary<string, double>> dictTF_IDF,
                                            double [] score, Dictionary<string , List<string>> dictTempsnippet)
    {
        List<DocsFinal> docs = new List<DocsFinal> ();

        double suma = 0;
        bool restriction = false;
        for(int i = 0; i < dictTF_IDF.Values.Count(); i++)
        {
            foreach(var valueD in dictTF_IDF.Values.ElementAt(i))
            {
                if(valueD.Value == 0)
                {
                    restriction = true;
                    break;
                }
                suma+=valueD.Value;
            }  
            if(restriction == false)
            {            
                score[i]+=(suma/(double)dictTF_IDF.Values.ElementAt(i).Count());
                if(score[i] > 0)
                {
                    DocsFinal dd = new DocsFinal();
                    dd.filename = dictTF_IDF.ElementAt(i).Key;

                    foreach(var frase in dictTempsnippet)
                    {
                        if(frase.Key == dd.filename)
                        {
                            dd.Frases = frase.Value;
                        }
                    }                                         
                    dd.score = (float)score[i];
                    docs.Add(dd);
                }
            }
            restriction = false;            
            suma = 0;
        }

        docs.Sort();
        docs.Reverse();
        
        return docs;
    }

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
    public static string BuildQuery(string query)
    {
        string res = "";
        List<string> result = new List<string>();
        result = query.Split(new char[] { '(', ')', ',', ';', '"', '.', '?', '-', ' ', '\0', '\n' })
        .Distinct().Where(d => !SplitList(d)).ToList<string>();
        if (result.Count() == 1)
            return result.ElementAt(0);
        for (int i = 0; i < result.Count(); i++)
        {
            if(result.ElementAt(i) == "*" || result.ElementAt(i) == "!" || result.ElementAt(i) == "^")
            {
                res += " " + result.ElementAt(i)+result.ElementAt(i+1);
                i++;
            }
            else
            {
                res += " " + result.ElementAt(i);
            }
        }

        return res;
    }
}
