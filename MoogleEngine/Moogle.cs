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
        //Parseando el query y quitando palabras sin relevancia.
        query = query.Trim();
        tempquery = BuildQuery(query);
        tempquery = tempquery.Trim();
        termsquery = tempquery.Split(new char[] {'(',')',',',';','"','.','?','-', ' ','\0','\n'})
        .Distinct().Where( d => !ComputeSnippets.SplitList(d)).ToList<string>();
        //Separando el operador de prioridad en caso de su uso.
        for (int i = 0; i < termsquery.Count; i++)
        {
            tmpPrioridad = termsquery[i].Split(new char[] {'*'}).ToList<string>(); 
            if(tmpPrioridad.Count > 1)
                dictPrioridad.Add(termsquery[i], 100);
            else
                dictPrioridad.Add(termsquery[i], 1);   

            termsquery[i] = tmpPrioridad[tmpPrioridad.Count-1].ToLower();
        }
        //Separando el operador de distancia en caso de su uso para el calculo del score.
        termsDistancia = query.Split(new char[] {'~'}).ToList<string>();
        List<string> tmp1 = new List<string>();
        List<string> tmp2 = new List<string>();
        if(termsDistancia.Count > 1 && !(termsDistancia[0] == "" || termsDistancia[1] == ""))
        {
            for (int i = 0; i < termsDistancia.Count - 1; i++)
            {
                tmp1 = termsDistancia[i].Split(new char[] {'(',')',',',';','"','.','?','-', ' ','\0','\n'})
                .Distinct().Where( d => !ComputeSnippets.SplitList(d)).ToList<string>();
                tmp2 = termsDistancia[i+1].Split(new char[] {'(',')',',',';','"','.','?','-', ' ','\0','\n'})
                .Distinct().Where(d => !ComputeSnippets.SplitList(d)).ToList<string>();
                dictDistancia.Add(tmp1[0].ToLower(), tmp2[tmp2.Count-1].ToLower());
                dictDistancia.Add(tmp2[tmp2.Count-1].ToLower(), tmp1[0].ToLower());
            }
        }
        else
        {
            termsDistancia.Remove("");
        }
        //LLamada al método para calcular el score de los documentos.
        score = computeScores.computeScore(sourcepath, query, termsquery, dictDistancia, dictPrioridad, dictTempsnippet, dictTF_IDF);
        //LLamada al método para la construcción de la clase DocsFinal que almacena
        //la información de cada documento para después armar los items.
        docs = doRanking(dictTF_IDF, score, dictTempsnippet);

        SearchItem[] items = new SearchItem[docs.Count()];

        for(int i = docs.Count-1; i >= 0; i--) 
        {
            //Se arman los items para después devolverlos.
            if(docs[i].Frases.Count() > 0)
                items[i] = new SearchItem(docs[i].filename ,docs[i].Frases[0], docs[i].score);
            else 
                items[i] = new SearchItem(docs[i].filename ,"", docs[i].score);

        }
        //Si en la búsqueda no se encuentra nada entonces se pasa a la construcción de items con frases parecidas.
        if (docs.Count() == 0)
        {
            score = computeScores.computeScoreLevensh(sourcepath, query, termsquery, dictDistancia, dictPrioridad, dictTempsnippet, dictTF_IDF);
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
            suggestion = Levenshtein.Suggestion();
        }
        //Si no se encuentra nada se devuelve lo que aparece a continuación. 
        if (docs.Count() <= 0)
        {
            SearchItem[] items1 = new SearchItem[1];
            items1[0] = new SearchItem(" ","Su busqueda no tiene relevancia. Pruebe con otra busqueda",0);
            items = items1;
        }
        return new SearchResult(items , suggestion);
    }
    
    /// <summary>
    /// Método donde se realiza el cálculo del operador de Distancia (~).
    /// </summary>
    /// <param name="dictDistancia"></param>
    /// <param name="content"></param>
    /// <param name="termsquery"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Método donde se almacenan los datos de cada documento en la clase DocsFinal.
    /// </summary>
    /// <param name="dictTF_IDF"></param>
    /// <param name="score"></param>
    /// <param name="dictTempsnippet"></param>
    /// <returns></returns>
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
    /// <summary>
    /// Método se parsea el query para eliminar espacios innecesarios.
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public static string BuildQuery(string query)
    {
        string res = "";
        List<string> result = new List<string>();
        result = query.Split(new char[] { '(', ')', ',', ';', '"', '.', '?', '-', ' ', '\0', '\n' })
        .Distinct().Where(d => !ComputeSnippets.SplitList(d)).ToList<string>();
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
