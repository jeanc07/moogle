public class Levenshtein
{
    private static string suggestion;
    private static double minDistance;

    public static string Suggestion ()
    {
        return suggestion;
    }

    /// <summary>
    /// Algoritmo donde se calcula la Distancia de Damareau-Levenshtein.
    /// </summary>
    /// <param name="string1"></Consulta>
    /// <param name="string2"></Un string "x" para comparar>
    /// <param name="threshold"></Umbral de búsqueda>
    /// <returns></Retorna la menor distancia existente en un documento entre ambas cadenas>
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
    /// <summary>
    /// Método donde se determina las palabras o frases más parecidas y se devuelve la que menor distancia tenga.
    /// </summary>
    /// <param name="query"></Consulta>
    /// <param name="frase"></Una frase o palabra para comparar>
    /// <returns></Devuelve la frase o palabra más parecida>
    public static bool Calculatequery(string query, string frase)
    {
        minDistance = 9999;
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
}