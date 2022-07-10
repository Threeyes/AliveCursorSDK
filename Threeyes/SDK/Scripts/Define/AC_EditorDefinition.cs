public static class AC_EditorDefinition
{
	// ——Editor Path
	///PS:
	///1.Use the origin File name because that's what will show on Inspector
	///2.Should Match the path to SDK/../Component

	public const string Menu_AliveCursor = "Alive Cursor/";

	//##
	public const string MenuElement_Cursor = "Cursor/";
	public const string MenuElement_Common = "Common/";
	public const string MenuElement_Workshop = "Workshop/";
	public const string MenuElement_Config = "Config/";
	public const string MenuElement_Persistent = "Persistent/";

	//###
	public const string MenuElement_Controller = "Controller/";
	public const string MenuElement_Behaviour = "Behaviour/";

	//####
	public const string MenuElement_Environment = "Environment/";
	public const string MenuElement_Input = "Input/";
	public const string MenuElement_Appearance = "Appearance/";
	public const string MenuElement_State = "State/";
	public const string MenuElement_Transform = "Transform/";

	//##Others
	public const string MenuElement_SO = "SO/";
	public const string MenuElement_Action = "Action/";

	//——Component Menu——
	public const string ComponentMenuPrefix_Root = Menu_AliveCursor;
	public const string ComponentMenuPrefix_AC_Cursor = ComponentMenuPrefix_Root + MenuElement_Cursor;
	public const string ComponentMenuPrefix_AC_Cursor_Controller = ComponentMenuPrefix_AC_Cursor + MenuElement_Controller;
	public const string ComponentMenuPrefix_AC_Cursor_Controller_Environment = ComponentMenuPrefix_AC_Cursor_Controller + MenuElement_Environment;
	public const string ComponentMenuPrefix_AC_Cursor_Controller_State = ComponentMenuPrefix_AC_Cursor_Controller + MenuElement_State;
	public const string ComponentMenuPrefix_AC_Cursor_Controller_Transform = ComponentMenuPrefix_AC_Cursor_Controller + MenuElement_Transform;
	public const string ComponentMenuPrefix_AC_Cursor_Behaviour = ComponentMenuPrefix_AC_Cursor + MenuElement_Behaviour;
	public const string ComponentMenuPrefix_AC_Cursor_Behaviour_Input = ComponentMenuPrefix_AC_Cursor_Behaviour + MenuElement_Input;
	public const string ComponentMenuPrefix_AC_Cursor_Behaviour_Appearance = ComponentMenuPrefix_AC_Cursor_Behaviour + MenuElement_Appearance;
	public const string ComponentMenuPrefix_AC_Cursor_Behaviour_State = ComponentMenuPrefix_AC_Cursor_Behaviour + MenuElement_State;
	public const string ComponentMenuPrefix_Persistent = ComponentMenuPrefix_Root + MenuElement_Persistent;


	//——Hierarchy Menu——
	public const string Hierarchy_Root = "GameObject/";
	public const string HierarchyMenuPrefix_Root = Hierarchy_Root + Menu_AliveCursor;
	public const string HierarchyMenuPrefix_Cursor_Controller_Environment = Hierarchy_Root + ComponentMenuPrefix_AC_Cursor_Controller_Environment;
	public const string HierarchyMenuPrefix_Cursor_Controller_State = Hierarchy_Root + ComponentMenuPrefix_AC_Cursor_Controller_State;
	public const string HierarchyMenuPrefix_Cursor_Controller_Transform = Hierarchy_Root + ComponentMenuPrefix_AC_Cursor_Controller_Transform;
	public const string HierarchyMenuPrefix_Cursor_Behaviour_Input = Hierarchy_Root + ComponentMenuPrefix_AC_Cursor_Behaviour_Input;
	public const string HierarchyMenuPrefix_Cursor_Behaviour_Appearance = Hierarchy_Root + ComponentMenuPrefix_AC_Cursor_Behaviour_Appearance;
	public const string HierarchyMenuPrefix_Cursor_Behaviour_State = Hierarchy_Root + ComponentMenuPrefix_AC_Cursor_Behaviour_State;
	public const string HierarchyMenuPrefix_Persistent = Hierarchy_Root + ComponentMenuPrefix_Persistent;

	//——Asset Menu——
	///PS:
	///1.因为Asset菜单默认是创建文件，因此不需要增加SO的前缀
	public const string AssetMenuPrefix_Root = Menu_AliveCursor;

	public const string AssetMenuPrefix_AC_Cursor = ComponentMenuPrefix_AC_Cursor;
	public const string AssetMenuPrefix_AC_Cursor_Behaviour_Input = ComponentMenuPrefix_AC_Cursor_Behaviour_Input;
	public const string AssetMenuPrefix_AC_Cursor_Behaviour_Appearance = ComponentMenuPrefix_AC_Cursor_Behaviour_Appearance;
	public const string AssetMenuPrefix_AC_Cursor_Behaviour_State = ComponentMenuPrefix_AC_Cursor_Behaviour_State;
	public const string AssetMenuPrefix_AC_Cursor_Controller = ComponentMenuPrefix_AC_Cursor_Controller;
	public const string AssetMenuPrefix_AC_Cursor_Controller_Environment = ComponentMenuPrefix_AC_Cursor_Controller_Environment;
	public const string AssetMenuPrefix_AC_Cursor_Controller_State = ComponentMenuPrefix_AC_Cursor_Controller_State;
	public const string AssetMenuPrefix_AC_Cursor_Controller_State_Action = AssetMenuPrefix_AC_Cursor_Controller_State + MenuElement_Action;
	public const string AssetMenuPrefix_AC_Cursor_Controller_Transform = ComponentMenuPrefix_AC_Cursor_Controller_Transform;

	public const string AssetMenuPrefix_AC_Common = AssetMenuPrefix_Root + MenuElement_Common;
	public const string AssetMenuPrefix_AC_Workshop = AssetMenuPrefix_Root + MenuElement_Workshop;
	public const string AssetMenuPrefix_AC_Persistent = AssetMenuPrefix_Root + MenuElement_Persistent;
	public const string AssetMenuPrefix_AC_Config = AssetMenuPrefix_Root + MenuElement_Config;

	public const string AssetMenuPrefix_SO_Action = AssetMenuPrefix_Root + MenuElement_Action;
	public const string AssetMenuPrefix_SO_Action_Common = AssetMenuPrefix_SO_Action + MenuElement_Common;
}
//测试