using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class LexItem
    {
        public string Value;
        public MultiLanguageString Text;
        public List<LexCategoryDesignation> CategoryDesignations;

        public LexItem(string value, MultiLanguageString text, List<LexCategoryDesignation> categoryDesignations)
        {
            Value = value;
            Text = text;
            CategoryDesignations = categoryDesignations;
        }

        public Designator FindCategoryDesignation(string categoryString)
        {
            if (CategoryDesignations == null)
                return null;

            LexCategoryDesignation categoryDesignation = CategoryDesignations.FirstOrDefault(x => x.Category == categoryString);

            if (categoryDesignation != null)
                return categoryDesignation.Designation;

            return null;
        }

        public List<Designator> FindCategoryDesignations(string categoryString)
        {
            if (CategoryDesignations == null)
                return null;

            List<Designator> designators = null;

            foreach (LexCategoryDesignation lcd in CategoryDesignations)
            {
                if (lcd.Category == categoryString)
                {
                    if (designators == null)
                        designators = new List<Designator>() { lcd.Designation };
                    else
                        designators.Add(lcd.Designation);
                }
            }

            if (categoryString.Contains(","))
            {
                string[] parts = categoryString.Split(LanguageLookup.Comma, StringSplitOptions.RemoveEmptyEntries);

                foreach (string part in parts)
                {
                    foreach (LexCategoryDesignation lcd in CategoryDesignations)
                    {
                        if (lcd.Category == part)
                        {
                            if (designators == null)
                                designators = new List<Designator>() { lcd.Designation };
                            else
                                designators.Add(lcd.Designation);
                        }
                    }
                }
            }

            return designators;
        }

        public int CategoryDesignationCount()
        {
            if (CategoryDesignations == null)
                return 0;

            return CategoryDesignations.Count();
        }
    }
}
