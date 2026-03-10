using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

/// @author gr313135
/// @version 06.03.2026
/// <summary>
/// 
/// </summary>
public class Bussipysäkit
{
    /// <summary>
    /// 
    /// </summary>
    public static void Main()
    {
        StringBuilder jono = new StringBuilder("makaronilaatikko on kotiruokaa");
        int poistettu = Poista(jono, 'a');
        Console.WriteLine($"Jono nyt: {jono}, kirjaimia poistettu: {poistettu}");
    }

    /// <example>
    /// <pre name="test">
    /// StringBuilder jono  ===  "kissa istuu puussa"; // alustetaan jono
    /// Poista(jono, ' ') === 2; jono.ToString() === "kissaistuupuussa";
    /// Poista(jono, 'u') === 4; jono.ToString() === "kissaistpssa";
    /// Poista(jono, 'k') === 1; jono.ToString() === "issaistpssa";
    /// Poista(jono, 'a') === 2; jono.ToString() === "issistpss";
    /// Poista(jono, 's') === 5; jono.ToString() === "iitp";
    /// Poista(jono, 'x') === 0; jono.ToString() === "iitp";
    /// </pre>
    /// </example>
    public static int Poista(StringBuilder jono, char poistettava)
    {
        int poistettu = 0;
        int i = 0;
        string jono2 = jono.ToString();

        foreach (char merkki in jono2)
        {
            if (merkki == poistettava)
            {
                poistettu++;
                jono.Remove(i, 1);
            }

            i++;
        }
        
        return poistettu;
    }

    
}