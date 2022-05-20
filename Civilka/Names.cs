using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Civilka {
     class Names {

        /* Name creation
        1.0 Country influenced:
        > Names of the cultures are sometimes the same of the countries 
        > For civs ending with -land it gets usually changed to -(?)ish (english, polish, scottish)
        > Making it end with -ian is very common (Estonia - Estonian, Hungary - Hungarian)
        2.0 Generated:
        > Name of randomly generated culture can start with any letter really
        > It is common that there is at least one vowel in every 2-3 letter block in name (Cad|do, Ga|mi|la|ra|ay, Bur|me|se)
        > Ending alter by adding -n to vowels and -i to other
        */



        static string vowels = "aeiou";
        static string consonants = "bcdfghjklmnpqrstvwxyz";
        private static readonly string letters = vowels + consonants;
        static readonly string[] threeLetters = new string[]{ "shi", "chu" };
        public static char getRandomLetter(ref string text) {
            return text[Misc.getRandomInt(0, text.Length - 1)];
        }

        static string getRandomThreeBlock() {
            string block = "";
            for (int i = 0; i < 3; i++) {
                char letter;
                bool useVowel = Misc.getRandomInt(0, 4) < (i * 0.25);
                if (useVowel) {
                    letter = getRandomLetter(ref vowels);
                } else {
                    letter = getRandomLetter(ref consonants);
                }
                // Add
                block += letter;
            }
            return block;
        }

        

    }
}
