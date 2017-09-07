using System;
using System.Drawing;
using Grasshopper.Kernel;

namespace AgentSystem
{
    public class AgentSystemInfo : GH_AssemblyInfo
    {
        public override string Name
        {
            get
            {
                return "AgentSystem";
            }
        }
        public override Bitmap Icon
        {
            get
            {
                //Return a 24x24 pixel bitmap to represent this GHA library.
                return null;
            }
        }
        public override string Description
        {
            get
            {
                //Return a short string describing the purpose of this GHA library.
                return "";
            }
        }
        public override Guid Id
        {
            get
            {
                return new Guid("2638ef42-8148-4987-a5c3-fcf6fbc96422");
            }
        }

        public override string AuthorName
        {
            get
            {
                //Return a string identifying you or your company.
                return "";
            }
        }
        public override string AuthorContact
        {
            get
            {
                //Return a string representing your preferred contact details.
                return "";
            }
        }
    }
}
