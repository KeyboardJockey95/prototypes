using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.ObjectInterfaces;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Media;
using JTLanguageModelsPortable.Matchers;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public class PortugueseTool : LanguageTool
    {
        public PortugueseTool() : base(LanguageLookup.Portuguese)
        {
            _UsesImplicitPronouns = true;
        }

        public override IBaseObject Clone()
        {
            return new PortugueseTool();
        }

        // From https://spanishdictionary.cc/common-spanish-abbreviations
        public static Dictionary<string, string> PortugueseAbbreviationDictionary = new Dictionary<string, string>()
        {
            { "a.C.", "antes de Cristo" },              // B.C.
            { "a. de C.", "antes de Cristo" },          // B.C.
            { "a.J.C.", "antes de Jesucristo" },        // B.C.
            { "a. de J.C.", "antes de Jesucristo" },    // B.C.
            { "a. m.", "antes del mediodía" },          // a.m before noon
            { "apdo.", "apartado" },                    // postal P.O Box
            { "aprox.", "aproximadamente" },            // approximately
            { "Av.", "avenida" },                       // Ave.avenue, in addresses
            { "Avda.", "avenida" },                     // Ave.avenue, in addresses
            { "Bs. As.", "Buenos Aires" },              // Buenos Aires
            { "c.c.", "centímetros cúbicos c.c." },     // cubic centimeters
            { "Cía", "compañía" },                      // Co. company
            { "cm", "centímetros" },                    // cm  centimeters
            { "c/u", "cada uno" },                      // apiece
            { "D.", "don" },                            // Sir
            { "Da", "doña" },                           // Madam
            { "d.C.", "después de Cristo" },    		// A.D.
            { "d. de C.", "después de Cristo" },        // A.D.
            { "d.J.C.", "después de Jesucristo" },      // A.D.
            { "d. de J.C.", "después de Jesucristo" },  // A.D.
            { "dna.", "docena" },                       // dozen
            { "EE. UU.", "Estados Unidos" },            // U.S
            { "esq.", "esquina" },                      // street corner
            { "etc.", "etcétera" },                     // etc.
            { "f.c.", "ferrocarril R.R." },             // railroad
            { "F.C.", "ferrocarril R.R." },             // railroad
            { "FF. AA.", "fuerzas armadas" },           // armed forces
            { "Dr.", "doctor" },                        // Dr.
            { "Dra.", "doctora" },                      // Dr.
            { "E", "este (punto cardinal)" },           // E   east
            { "Gob.", "gobierno" },                     // Gov.
            { "km/h", "kilómetros por hora" },          // kilometers per hour
            { "l", "litros" },                          // liters
            { "Lic.", "licenciado" },                   // attorney
            { "m", "metros" },                          // meters
            { "mm", "milímetros" },                     // millimeters
            { "h", "hora" },                            // hour
            { "Ing.", "ingeniero" },                    // engineer
            { "kg", "kilogramos" },                     // kg  kilograms
            { "pág.", "página" },                       // page
            { "N", "norte" },                           // N   north
            // Too easily mistaken for negative
            // { "no., núm.", "número" },                  // No. number
            { "O", "oeste" },                           // W   west
            { "P.D.", "postdata" },                     // P.S.
            { "OEA", "Organización de Estados Americanos" },            // OAS Organization of American States
            { "ONU", "Organización de Naciones Unidas" },               // UN  United Nations
            { "OTAN", "La Organización del Tratado Atlántico Norte" },  // NATO    North Atlantic Treaty Organization
            { "p.ej.", "por ejemplo" },                 // e.g	for example
            { "p. m.", "post meridien" },               // p.m after noon
            { "Prof.", "profesor" },                    // Professor
            { "Profa.", "profesora" },                  // Professor
            { "q.e.p.d.", "que en paz descanse" },      // R.I.P   rest in peace
            { "S", "sur S" },                           // south
            { "S.A.", "Sociedad Anónima" },             // Inc.
            { "S.L.", "Sociedad Limitada" },            // Ltd.
            { "Sr.", "señor" },                         // Mr.
            { "Sra.", "señora" },                       // Mrs., Ms.
            { "Srta.", "señorita" },                    // Miss, Ms.
            { "Ud.", "usted" },                         // You
            { "Vd.", "usted" },                         // You
            { "Uds.", "ustedes" },                      // You all
            { "Vds.", "ustedes" },                      // You all
            { "vol.", "volumen" },                      // vol.    volume
            { "W.C", "water closet" },                  // bathroom, toilet
        };

        public override Dictionary<string, string> AbbreviationDictionary
        {
            get
            {
                return PortugueseAbbreviationDictionary;
            }
        }
    }
}
