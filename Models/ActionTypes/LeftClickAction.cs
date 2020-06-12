namespace Models.ActionTypes
{
    public class LeftClickAction : BaseActionType
    {
        public LeftClickAction(
            string name = "LeftClick", 
            string description = "Левый клик мыши", 
            string scriptPath = "") 
            : base(name, description, scriptPath)
        {
        }
    }
}