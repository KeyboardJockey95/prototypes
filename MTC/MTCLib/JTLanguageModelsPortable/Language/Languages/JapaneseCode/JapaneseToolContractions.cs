using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;
using JTLanguageModelsPortable.Dictionary;
using JTLanguageModelsPortable.Converters;
using JTLanguageModelsPortable.Application;

namespace JTLanguageModelsPortable.Language
{
    public partial class JapaneseToolCode : JapaneseTool
    {

        public override bool Contract(
            MultiLanguageString uncontracted,
            out List<MultiLanguageString> contracted)
        {
            string kanji = uncontracted.Text(JapaneseID);
            bool returnValue = false;
            contracted = null;
            if (kanji.Contains("でわないか"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                Replace(mls, "でわないか", "でわないか", "dewanaika", "じゃん", "じゃん", "jan");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.Contains("じゃないか"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                Replace(mls, "じゃないか", "じゃないか", "janaika", "じゃん", "じゃん", "jan");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("てしまう"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "てしまう", "てしまう", "teshimau", "ちゃう", "ちゃう", "chau");
                contracted.Add(mls);
                mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "てしまう", "てしまう", "teshimau", "ちまう", "ちまう", "chimau");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("でしまう"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "でしまう", "でしまう", "deshimau", "じまう", "じまう", "jimau");
                contracted.Add(mls);
                mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "でしまう", "でしまう", "deshimau", "ぢまう", "ぢまう", "jimau");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("ている"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "ている", "ている", "teiru", "てる", "てる", "teru");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("ておく"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "ておく", "ておく", "teoku", "とく", "とく", "toku");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("てはしない"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "てはしない", "てはしない", "tewashinai", "てやしない", "てやしない", "teyashinai");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("てあげる"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "てあげる", "てあげる", "teageru", "たげる", "たげる", "tageru");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("ては"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "ては", "ては", "tewa", "ちゃ", "ちゃ", "cha");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("では"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "では", "では", "dewa", "じゃ", "じゃ", "ja");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("るの"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "るの", "るの", "runo", "んの", "んの", "nno");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("らない"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "らない", "らない", "ranai", "んない", "んない", "nnai");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("なければ"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "なければ", "なければ", "nakareba", "なきゃ", "なきゃ", "nakya");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("なくては"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "なくては", "なくては", "nakutewa", "なくちゃ", "なくちゃ", "nakucha");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("ではないか"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "ではないか", "ではないか", "dewanaika", "じゃん", "じゃん", "jan");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("じゃないか"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "じゃないか", "じゃないか", "janaika", "じゃん", "じゃん", "jan");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("ない"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "ない", "ない", "nai", "ねえ", "ねえ", "nee");
                contracted.Add(mls);
                mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "ない", "ない", "nai", "ねー", "ねー", "nee");
                contracted.Add(mls);
                mls = new MultiLanguageString(uncontracted);
                ReplaceSuffix(mls, "ない", "ない", "nai", "ん", "ん", "n");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.Contains("これは"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                Replace(mls, "これは", "これは", "korewa", "こりゃ", "こりゃ", "korya");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.Contains("それは"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                Replace(mls, "それは", "それは", "sorewa", "そりゃ", "そりゃ", "sorya");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.Contains("あれは"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                Replace(mls, "あれは", "あれは", "arewa", "ありゃ", "ありゃ", "arya");
                contracted.Add(mls);
                returnValue = true;
            }
            else if (kanji.Contains("でも"))
            {
                contracted = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(uncontracted);
                Replace(mls, "でも", "でも", "demo", "だって", "だって", "datte");
                contracted.Add(mls);
                returnValue = true;
            }
            return returnValue;
        }

        public override bool ExpandContraction(
            MultiLanguageString possiblyContracted,
            out List<MultiLanguageString> expanded)
        {
            string kanji = possiblyContracted.Text(JapaneseID);
            bool returnValue = false;
            expanded = null;
            if (kanji.EndsWith("ちゃう"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "ちゃう", "ちゃう", "chau", "てしまう", "てしまう", "teshimau");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("ちまう"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "ちまう", "ちまう", "chimau", "てしまう", "てしまう", "teshimau");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("じまう"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "じまう", "じまう", "jimau", "でしまう", "でしまう", "deshimau");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("ぢまう"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "ぢまう", "ぢまう", "jimau", "でしまう", "でしまう", "deshimau");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("てる"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "てる", "てる", "teru", "ている", "ている", "teiru");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("とく"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "とく", "とく", "toku", "ておく", "ておく", "teoku");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("てやしない"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "てやしない", "てやしない", "teyashinai", "てはしない", "てはしない", "tewashinai");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("たげる"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "たげる", "たげる", "tageru", "てあげる", "てあげる", "teageru");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("ちゃ"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "ちゃ", "ちゃ", "cha", "ては ", "ては", "tewa");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("じゃ"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "じゃ", "じゃ", "ja", "では", "では", "dewa");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("んない"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "んない", "んない", "nnai", "らない", "らない", "ranai");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("なきゃ"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "なきゃ", "なきゃ", "nakya", "なければ", "なければ", "nakereba");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("なくちゃ"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "なくちゃ", "なくちゃ", "nakucha", "なくては", "なくては", "nakutewa");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("じゃん"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "じゃん", "じゃん", "jan", "でわないか", "でわないか", "dewanaika");
                expanded.Add(mls);
                mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "じゃん", "じゃん", "jan", "じゃないか", "じゃないか", "janaika");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("んの"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "んの", "んの", "nno", "るの", "るの", "runo");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.EndsWith("ん"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                ReplaceSuffix(mls, "ん", "ん", "n", "ない", "ない", "nai");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.Contains("これは"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                Replace(mls, "こりゃ", "こりゃ", "korya", "これは", "これは", "korewa");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.Contains("それは"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                Replace(mls, "そりゃ", "そりゃ", "sorya", "それは", "それは", "sorewa");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.Contains("あれは"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                Replace(mls, "ありゃ", "ありゃ", "arya", "あれは", "あれは", "arewa");
                expanded.Add(mls);
                returnValue = true;
            }
            else if (kanji.Contains("でも"))
            {
                expanded = new List<MultiLanguageString>();
                MultiLanguageString mls = new MultiLanguageString(possiblyContracted);
                Replace(mls, "だって", "だって", "datte", "でも", "でも", "demo");
                expanded.Add(mls);
                returnValue = true;
            }
            return returnValue;
        }
    }
}
