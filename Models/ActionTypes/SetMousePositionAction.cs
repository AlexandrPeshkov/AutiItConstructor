namespace Models.ActionTypes
{
    public class SetMousePositionAction : BaseActionType
    {
        public SetMousePositionAction(
            string name = "SetMousePosition", 
            string description = "Поставить курсор в позицию", 
            string scriptPath = "")
            : base(name, description, scriptPath)
        {
        }
    }
}
