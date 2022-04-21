using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using JTLanguageModelsPortable.Object;

namespace JTLanguageModelsPortable.Language
{
    public class SentenceFix : BaseObjectKeyed
    {
        public string Label;
        public List<string> Input;
        public List<string> Output;

        public SentenceFix(
            string key,
            string label,
            List<string> input,
            List<string>output) : base(key)
        {
            Label = label;
            Input = input;
            Output = output;
        }

        public SentenceFix(XElement element)
        {
            OnElement(element);
        }

        public SentenceFix()
        {
            Label = null;
            Input = null;
            Output = null;
        }

        public int InputCount
        {
            get
            {
                if (Input == null)
                    return 0;
                return Input.Count();
            }
        }

        public string GetInputIndexed(int index)
        {
            if (Input == null)
                return null;
            if ((index < 0) || (index >= Input.Count()))
                return null;
            return Input[index];
        }

        public int OutputCount
        {
            get
            {
                if (Output == null)
                    return 0;
                return Output.Count();
            }
        }

        public string GetOutputIndexed(int index)
        {
            if (Output == null)
                return null;
            if ((index < 0) || (index >= Output.Count()))
                return null;
            return Output[index];
        }

        public bool IsSplit()
        {
            if (InputCount > 1)
                return false;

            if (OutputCount <= 1)
                return false;

            return true;
        }

        public bool IsJoin()
        {
            if (Input == null)
                return false;

            if (Input.Count() <= 1)
                return false;

            return true;
        }

        public override XElement GetElement(string name)
        {
            XElement element = new XElement(name);

            if (!String.IsNullOrEmpty(Label))
                element.Add(new XElement("Label", Label));

            if ((Input != null) && (Input.Count() != 0))
            {
                foreach (string input in Input)
                    element.Add(new XElement("Input", input));
            }

            if ((Output != null) && (Output.Count() != 0))
            {
                foreach (string output in Output)
                    element.Add(new XElement("Output", output));
            }

            return element;
        }

        public override bool OnChildElement(XElement childElement)
        {
            switch (childElement.Name.LocalName)
            {
                case "Label":
                    Label = childElement.Value.Trim();
                    break;
                case "Input":
                    if (Input == null)
                        Input = new List<string>();
                    Input.Add(childElement.Value);
                    if (KeyString != Input.First())
                        Key = Input.First();
                    break;
                case "Output":
                    if (Output == null)
                        Output = new List<string>();
                    Output.Add(childElement.Value);
                    break;
                default:
                    return base.OnChildElement(childElement);
            }

            return true;
        }
    }
}
