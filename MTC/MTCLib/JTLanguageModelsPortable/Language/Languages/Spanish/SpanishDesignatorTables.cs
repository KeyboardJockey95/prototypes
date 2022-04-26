using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JTLanguageModelsPortable.Language
{
    public class SpanishDesignatorTable : DesignatorTable
    {
        public SpanishDesignatorTable()
        {
            Add("Special", "Dictionary");
            Add("Special", "Infinitive");
            Add("Special", "Gerund");
            Add(String.Empty, "Past", String.Empty, "Masculine", String.Empty, String.Empty, "Singular", "Participle");
            Add(String.Empty, "Past", String.Empty, "Feminine", String.Empty, String.Empty, "Singular", "Participle");
            Add(String.Empty, "Past", String.Empty, "Masculine", String.Empty, String.Empty, "Plural", "Participle");
            Add(String.Empty, "Past", String.Empty, "Feminine", String.Empty, String.Empty, "Plural", "Participle");

            Add("Indicative", "Present", String.Empty, String.Empty, String.Empty,"First", "Singular", String.Empty);
            Add("Indicative", "Present", String.Empty, String.Empty, String.Empty,"Second", "Singular", String.Empty);
            Add("Indicative", "Present", String.Empty, String.Empty, String.Empty,"Third", "Singular", String.Empty);
            Add("Indicative", "Present", String.Empty, String.Empty, String.Empty,"First", "Plural", String.Empty);
            Add("Indicative", "Present", String.Empty, String.Empty, String.Empty,"Second", "Plural", String.Empty);
            Add("Indicative", "Present", String.Empty, String.Empty, String.Empty,"Third", "Plural", String.Empty);

            Add("Indicative", "Present", String.Empty, String.Empty, "Negative", "First", "Singular", String.Empty);
            Add("Indicative", "Present", String.Empty, String.Empty, "Negative", "Second", "Singular", String.Empty);
            Add("Indicative", "Present", String.Empty, String.Empty, "Negative", "Third", "Singular", String.Empty);
            Add("Indicative", "Present", String.Empty, String.Empty, "Negative", "First", "Plural", String.Empty);
            Add("Indicative", "Present", String.Empty, String.Empty, "Negative", "Second", "Plural", String.Empty);
            Add("Indicative", "Present", String.Empty, String.Empty, "Negative", "Third", "Plural", String.Empty);

            Add("Indicative", "Past", "Perfect", String.Empty, String.Empty,"First", "Singular", String.Empty);
            Add("Indicative", "Past", "Perfect", String.Empty, String.Empty,"Second", "Singular", String.Empty);
            Add("Indicative", "Past", "Perfect", String.Empty, String.Empty,"Third", "Singular", String.Empty);
            Add("Indicative", "Past", "Perfect", String.Empty, String.Empty,"First", "Plural", String.Empty);
            Add("Indicative", "Past", "Perfect", String.Empty, String.Empty,"Second", "Plural", String.Empty);
            Add("Indicative", "Past", "Perfect", String.Empty, String.Empty,"Third", "Plural", String.Empty);

            Add("Indicative", "Past", "Perfect", String.Empty, "Negative", "First", "Singular", String.Empty);
            Add("Indicative", "Past", "Perfect", String.Empty, "Negative", "Second", "Singular", String.Empty);
            Add("Indicative", "Past", "Perfect", String.Empty, "Negative", "Third", "Singular", String.Empty);
            Add("Indicative", "Past", "Perfect", String.Empty, "Negative", "First", "Plural", String.Empty);
            Add("Indicative", "Past", "Perfect", String.Empty, "Negative", "Second", "Plural", String.Empty);
            Add("Indicative", "Past", "Perfect", String.Empty, "Negative", "Third", "Plural", String.Empty);

            Add("Indicative", "Past", "Imperfect", String.Empty, String.Empty,"First", "Singular", String.Empty);
            Add("Indicative", "Past", "Imperfect", String.Empty, String.Empty,"Second", "Singular", String.Empty);
            Add("Indicative", "Past", "Imperfect", String.Empty, String.Empty,"Third", "Singular", String.Empty);
            Add("Indicative", "Past", "Imperfect", String.Empty, String.Empty,"First", "Plural", String.Empty);
            Add("Indicative", "Past", "Imperfect", String.Empty, String.Empty,"Second", "Plural", String.Empty);
            Add("Indicative", "Past", "Imperfect", String.Empty, String.Empty,"Third", "Plural", String.Empty);

            Add("Indicative", "Past", "Imperfect", String.Empty, "Negative", "First", "Singular", String.Empty);
            Add("Indicative", "Past", "Imperfect", String.Empty, "Negative", "Second", "Singular", String.Empty);
            Add("Indicative", "Past", "Imperfect", String.Empty, "Negative", "Third", "Singular", String.Empty);
            Add("Indicative", "Past", "Imperfect", String.Empty, "Negative", "First", "Plural", String.Empty);
            Add("Indicative", "Past", "Imperfect", String.Empty, "Negative", "Second", "Plural", String.Empty);
            Add("Indicative", "Past", "Imperfect", String.Empty, "Negative", "Third", "Plural", String.Empty);

            Add("Indicative", "Future", String.Empty, String.Empty, String.Empty,"First", "Singular", String.Empty);
            Add("Indicative", "Future", String.Empty, String.Empty, String.Empty,"Second", "Singular", String.Empty);
            Add("Indicative", "Future", String.Empty, String.Empty, String.Empty,"Third", "Singular", String.Empty);
            Add("Indicative", "Future", String.Empty, String.Empty, String.Empty,"First", "Plural", String.Empty);
            Add("Indicative", "Future", String.Empty, String.Empty, String.Empty,"Second", "Plural", String.Empty);
            Add("Indicative", "Future", String.Empty, String.Empty, String.Empty,"Third", "Plural", String.Empty);

            Add("Indicative", "Future", String.Empty, String.Empty, "Negative", "First", "Singular", String.Empty);
            Add("Indicative", "Future", String.Empty, String.Empty, "Negative", "Second", "Singular", String.Empty);
            Add("Indicative", "Future", String.Empty, String.Empty, "Negative", "Third", "Singular", String.Empty);
            Add("Indicative", "Future", String.Empty, String.Empty, "Negative", "First", "Plural", String.Empty);
            Add("Indicative", "Future", String.Empty, String.Empty, "Negative", "Second", "Plural", String.Empty);
            Add("Indicative", "Future", String.Empty, String.Empty, "Negative", "Third", "Plural", String.Empty);

            Add("Conditional", String.Empty, String.Empty, String.Empty, String.Empty,"First", "Singular", String.Empty);
            Add("Conditional", String.Empty, String.Empty, String.Empty, String.Empty,"Second", "Singular", String.Empty);
            Add("Conditional", String.Empty, String.Empty, String.Empty, String.Empty,"Third", "Singular", String.Empty);
            Add("Conditional", String.Empty, String.Empty, String.Empty, String.Empty,"First", "Plural", String.Empty);
            Add("Conditional", String.Empty, String.Empty, String.Empty, String.Empty,"Second", "Plural", String.Empty);
            Add("Conditional", String.Empty, String.Empty, String.Empty, String.Empty,"Third", "Plural", String.Empty);

            Add("Conditional", String.Empty, String.Empty, String.Empty, "Negative", "First", "Singular", String.Empty);
            Add("Conditional", String.Empty, String.Empty, String.Empty, "Negative", "Second", "Singular", String.Empty);
            Add("Conditional", String.Empty, String.Empty, String.Empty, "Negative", "Third", "Singular", String.Empty);
            Add("Conditional", String.Empty, String.Empty, String.Empty, "Negative", "First", "Plural", String.Empty);
            Add("Conditional", String.Empty, String.Empty, String.Empty, "Negative", "Second", "Plural", String.Empty);
            Add("Conditional", String.Empty, String.Empty, String.Empty, "Negative", "Third", "Plural", String.Empty);

            Add("Subjunctive", "Present", String.Empty, String.Empty, String.Empty,"First", "Singular", String.Empty);
            Add("Subjunctive", "Present", String.Empty, String.Empty, String.Empty,"Second", "Singular", String.Empty);
            Add("Subjunctive", "Present", String.Empty, String.Empty, String.Empty,"Third", "Singular", String.Empty);
            Add("Subjunctive", "Present", String.Empty, String.Empty, String.Empty,"First", "Plural", String.Empty);
            Add("Subjunctive", "Present", String.Empty, String.Empty, String.Empty,"Second", "Plural", String.Empty);
            Add("Subjunctive", "Present", String.Empty, String.Empty, String.Empty,"Third", "Plural", String.Empty);

            Add("Subjunctive", "Present", String.Empty, String.Empty, "Negative", "First", "Singular", String.Empty);
            Add("Subjunctive", "Present", String.Empty, String.Empty, "Negative", "Second", "Singular", String.Empty);
            Add("Subjunctive", "Present", String.Empty, String.Empty, "Negative", "Third", "Singular", String.Empty);
            Add("Subjunctive", "Present", String.Empty, String.Empty, "Negative", "First", "Plural", String.Empty);
            Add("Subjunctive", "Present", String.Empty, String.Empty, "Negative", "Second", "Plural", String.Empty);
            Add("Subjunctive", "Present", String.Empty, String.Empty, "Negative", "Third", "Plural", String.Empty);

            Add("Subjunctive", "Past", "Imperfect1", String.Empty, String.Empty,"First", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect1", String.Empty, String.Empty,"Second", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect1", String.Empty, String.Empty,"Third", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect1", String.Empty, String.Empty,"First", "Plural", String.Empty);
            Add("Subjunctive", "Past", "Imperfect1", String.Empty, String.Empty,"Second", "Plural", String.Empty);
            Add("Subjunctive", "Past", "Imperfect1", String.Empty, String.Empty,"Third", "Plural", String.Empty);

            Add("Subjunctive", "Past", "Imperfect1", String.Empty, "Negative", "First", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect1", String.Empty, "Negative", "Second", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect1", String.Empty, "Negative", "Third", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect1", String.Empty, "Negative", "First", "Plural", String.Empty);
            Add("Subjunctive", "Past", "Imperfect1", String.Empty, "Negative", "Second", "Plural", String.Empty);
            Add("Subjunctive", "Past", "Imperfect1", String.Empty, "Negative", "Third", "Plural", String.Empty);

            Add("Subjunctive", "Past", "Imperfect2", String.Empty, String.Empty,"First", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect2", String.Empty, String.Empty,"Second", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect2", String.Empty, String.Empty,"Third", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect2", String.Empty, String.Empty,"First", "Plural", String.Empty);
            Add("Subjunctive", "Past", "Imperfect2", String.Empty, String.Empty,"Second", "Plural", String.Empty);
            Add("Subjunctive", "Past", "Imperfect2", String.Empty, String.Empty,"Third", "Plural", String.Empty);

            Add("Subjunctive", "Past", "Imperfect2", String.Empty, "Negative", "First", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect2", String.Empty, "Negative", "Second", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect2", String.Empty, "Negative", "Third", "Singular", String.Empty);
            Add("Subjunctive", "Past", "Imperfect2", String.Empty, "Negative", "First", "Plural", String.Empty);
            Add("Subjunctive", "Past", "Imperfect2", String.Empty, "Negative", "Second", "Plural", String.Empty);
            Add("Subjunctive", "Past", "Imperfect2", String.Empty, "Negative", "Third", "Plural", String.Empty);

            Add("Subjunctive", "Future", String.Empty, String.Empty, String.Empty,"First", "Singular", String.Empty);
            Add("Subjunctive", "Future", String.Empty, String.Empty, String.Empty,"Second", "Singular", String.Empty);
            Add("Subjunctive", "Future", String.Empty, String.Empty, String.Empty,"Third", "Singular", String.Empty);
            Add("Subjunctive", "Future", String.Empty, String.Empty, String.Empty,"First", "Plural", String.Empty);
            Add("Subjunctive", "Future", String.Empty, String.Empty, String.Empty,"Second", "Plural", String.Empty);
            Add("Subjunctive", "Future", String.Empty, String.Empty, String.Empty,"Third", "Plural", String.Empty);

            Add("Subjunctive", "Future", String.Empty, String.Empty, "Negative", "First", "Singular", String.Empty);
            Add("Subjunctive", "Future", String.Empty, String.Empty, "Negative", "Second", "Singular", String.Empty);
            Add("Subjunctive", "Future", String.Empty, String.Empty, "Negative", "Third", "Singular", String.Empty);
            Add("Subjunctive", "Future", String.Empty, String.Empty, "Negative", "First", "Plural", String.Empty);
            Add("Subjunctive", "Future", String.Empty, String.Empty, "Negative", "Second", "Plural", String.Empty);
            Add("Subjunctive", "Future", String.Empty, String.Empty, "Negative", "Third", "Plural", String.Empty);

            Add("Imperative", String.Empty, String.Empty, String.Empty, String.Empty,"Second", "Singular", String.Empty);
            Add("Imperative", String.Empty, String.Empty, String.Empty, String.Empty,"Third", "Singular", String.Empty);
            Add("Imperative", String.Empty, String.Empty, String.Empty, String.Empty,"First", "Plural", String.Empty);
            Add("Imperative", String.Empty, String.Empty, String.Empty, String.Empty,"Second", "Plural", String.Empty);
            Add("Imperative", String.Empty, String.Empty, String.Empty, String.Empty,"Third", "Plural", String.Empty);

            Add("Imperative", String.Empty, String.Empty, String.Empty, "Negative", "Second", "Singular", String.Empty);
            Add("Imperative", String.Empty, String.Empty, String.Empty, "Negative", "Third", "Singular", String.Empty);
            Add("Imperative", String.Empty, String.Empty, String.Empty, "Negative", "First", "Plural", String.Empty);
            Add("Imperative", String.Empty, String.Empty, String.Empty, "Negative", "Second", "Plural", String.Empty);
            Add("Imperative", String.Empty, String.Empty, String.Empty, "Negative", "Third", "Plural", String.Empty);
        }
    }
}
