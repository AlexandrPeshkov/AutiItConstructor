namespace Models.ActionTypes
{
    public class BaseActionType
    {
        public string Name { get; protected set; }

        public string Description { get; protected set; }

        public string ScriptPath { get; protected set; }

        public BaseActionType(string name, string description, string scriptPath)
        {
            Name = name;
            Description = description;
            ScriptPath = scriptPath;
        }
    }
}
