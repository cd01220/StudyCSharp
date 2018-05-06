namespace StudyCSharp
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Text;
    using System.Threading.Tasks;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json.Serialization;

    /// <summary>
    /// the entity class Student.
    /// </summary>
    public class Student
    {
        /// <summary>
        /// the person's roles
        /// </summary>
        private List<string> roles;

        /// <summary>
        /// keep the additional data.
        /// </summary>
        [JsonExtensionData]
        private IDictionary<string, JToken> additionalData;

        /// <summary>
        /// Initializes a new instance of the Student class.
        /// </summary>
        public Student() 
        {
            this.additionalData = new Dictionary<string, JToken>();
        }

        /// <summary>
        /// Gets or sets the Name of the student.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the Domain of the student.
        /// </summary>
        [JsonIgnore]
        public string Domain { get; set; }

        /// <summary>
        /// Gets or sets the Roles of the student.
        /// </summary>
        public List<string> Roles
        {
            get
            {
                if (this.roles == null)
                {
                    throw new Exception("Roles not loaded!");
                }

                return this.roles;
            }

            set 
            {
                this.roles = value; 
            }
        }

        /// <summary>
        /// Gets or sets the UserName of the student.
        /// these properties are set in OnDeserialized
        /// </summary>
        [JsonIgnore]
        public string UserName { get; set; }

        /// <summary>
        /// the OnDeserialized method.
        /// </summary>
        /// <param name="context">the context</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // SAMAccountName is not deserialized to any property
            // and so it is added to the extension data dictionary
            string samAccountName = (string)this.additionalData["SamAccountName"];

            string[] samAccountDic = samAccountName.Split('\\');
            this.Domain = samAccountDic[0];
            this.UserName = samAccountDic[1];
        }

        /// <summary>
        /// the OnError method.
        /// </summary>
        /// <param name="context">the context</param>
        /// /// <param name="errorContext">the errorContext</param>
        [OnError]
        private void OnError(StreamingContext context, ErrorContext errorContext)
        {
            errorContext.Handled = true;
        }
    }
}
