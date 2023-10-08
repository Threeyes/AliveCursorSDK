using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Threeyes.Steamworks
{
    /// <summary>
    /// ToUpdate:
    /// </summary>
    public static class Steamworks_EditorDefinition
    {
        // ——Editor Path
        ///PS:
        ///1.Use the origin File name because that's what will show on Inspector
        ///2.Should Match the path to SDK/../Component

        public const string Menu_Root = "Steamworks/";

        //##
        public const string MenuElement_Mod = "Mod/";
        public const string MenuElement_Common = "Common/";
        public const string MenuElement_Workshop = "Workshop/";
        public const string MenuElement_Persistent = "Persistent/";

        //###
        public const string MenuElement_Controller = "Controller/";
        public const string MenuElement_Behaviour = "Behaviour/";

        //####
        public const string MenuElement_Environment = "Environment/";
        public const string MenuElement_PostProcessing = "PostProcessing/";
        public const string MenuElement_Setting = "Setting/";
        public const string MenuElement_Input = "Input/";
        public const string MenuElement_Appearance = "Appearance/";
        public const string MenuElement_State = "State/";
        public const string MenuElement_Transform = "Transform/";
        public const string MenuElement_BoredAction = "BoredAction/";

        //##Others
        public const string MenuElement_Action = "Action/";

        //——Component Menu——
        public const string ComponentMenuPrefix_Root = Menu_Root;

        public const string ComponentMenuPrefix_Root_Mod = ComponentMenuPrefix_Root + MenuElement_Mod;
        public const string ComponentMenuPrefix_Root_Mod_Controller = ComponentMenuPrefix_Root_Mod + MenuElement_Controller;
        public const string ComponentMenuPrefix_Root_Mod_Controller_Environment = ComponentMenuPrefix_Root_Mod_Controller + MenuElement_Environment;
        public const string ComponentMenuPrefix_Root_Mod_Controller_PostProcessing = ComponentMenuPrefix_Root_Mod_Controller + MenuElement_PostProcessing;
        public const string ComponentMenuPrefix_Root_Mod_Controller_State = ComponentMenuPrefix_Root_Mod_Controller + MenuElement_State;
        public const string ComponentMenuPrefix_Root_Mod_Controller_Transform = ComponentMenuPrefix_Root_Mod_Controller + MenuElement_Transform;
        public const string ComponentMenuPrefix_Root_Mod_Controller_Transform_BoredAction = ComponentMenuPrefix_Root_Mod_Controller_Transform + MenuElement_BoredAction;
        public const string ComponentMenuPrefix_Root_Mod_Behaviour = ComponentMenuPrefix_Root_Mod + MenuElement_Behaviour;
        public const string ComponentMenuPrefix_Root_Mod_Behaviour_Setting = ComponentMenuPrefix_Root_Mod_Behaviour + MenuElement_Setting;
        public const string ComponentMenuPrefix_Root_Mod_Behaviour_Input = ComponentMenuPrefix_Root_Mod_Behaviour + MenuElement_Input;
        public const string ComponentMenuPrefix_Root_Mod_Behaviour_Appearance = ComponentMenuPrefix_Root_Mod_Behaviour + MenuElement_Appearance;
        public const string ComponentMenuPrefix_Root_Mod_Behaviour_State = ComponentMenuPrefix_Root_Mod_Behaviour + MenuElement_State;
        public const string ComponentMenuPrefix_Persistent = ComponentMenuPrefix_Root + MenuElement_Persistent;


        //——Hierarchy Menu——
        public const string Hierarchy_GameObject = "GameObject/";
        public const string HierarchyMenuPrefix_Mod_Controller_Environment = Hierarchy_GameObject + ComponentMenuPrefix_Root_Mod_Controller_Environment;
        public const string HierarchyMenuPrefix_Mod_Controller_PostProcessing = Hierarchy_GameObject + ComponentMenuPrefix_Root_Mod_Controller_PostProcessing;
        public const string HierarchyMenuPrefix_Mod_Controller_State = Hierarchy_GameObject + ComponentMenuPrefix_Root_Mod_Controller_State;
        public const string HierarchyMenuPrefix_Mod_Controller_Transform = Hierarchy_GameObject + ComponentMenuPrefix_Root_Mod_Controller_Transform;
        public const string HierarchyMenuPrefix_Mod_Controller_Transform_BoredAction = Hierarchy_GameObject + ComponentMenuPrefix_Root_Mod_Controller_Transform_BoredAction;
        public const string HierarchyMenuPrefix_Mod_Behaviour_Setting = Hierarchy_GameObject + ComponentMenuPrefix_Root_Mod_Behaviour_Setting;
        public const string HierarchyMenuPrefix_Mod_Behaviour_Input = Hierarchy_GameObject + ComponentMenuPrefix_Root_Mod_Behaviour_Input;
        public const string HierarchyMenuPrefix_Mod_Behaviour_Appearance = Hierarchy_GameObject + ComponentMenuPrefix_Root_Mod_Behaviour_Appearance;
        public const string HierarchyMenuPrefix_Mod_Behaviour_State = Hierarchy_GameObject + ComponentMenuPrefix_Root_Mod_Behaviour_State;
        public const string HierarchyMenuPrefix_Persistent = Hierarchy_GameObject + ComponentMenuPrefix_Persistent;

        //——Asset Menu——
        ///PS:
        ///1.因为Asset菜单默认是创建文件，因此不需要增加SO的前缀
        public const string AssetMenuPrefix_Root = Menu_Root;

        public const string AssetMenuPrefix_Root_Mod = ComponentMenuPrefix_Root_Mod;
        public const string AssetMenuPrefix_Root_Mod_Behaviour_Input = ComponentMenuPrefix_Root_Mod_Behaviour_Input;
        public const string AssetMenuPrefix_Root_Mod_Behaviour_Appearance = ComponentMenuPrefix_Root_Mod_Behaviour_Appearance;
        public const string AssetMenuPrefix_Root_Mod_Behaviour_State = ComponentMenuPrefix_Root_Mod_Behaviour_State;
        public const string AssetMenuPrefix_Root_Mod_Controller = ComponentMenuPrefix_Root_Mod_Controller;
        public const string AssetMenuPrefix_Root_Mod_Controller_Environment = ComponentMenuPrefix_Root_Mod_Controller_Environment;
        public const string AssetMenuPrefix_Root_Mod_Controller_PostProcessing = ComponentMenuPrefix_Root_Mod_Controller_PostProcessing;
        public const string AssetMenuPrefix_Root_Mod_Controller_State = ComponentMenuPrefix_Root_Mod_Controller_State;
        public const string AssetMenuPrefix_Root_Mod_Controller_State_Action = AssetMenuPrefix_Root_Mod_Controller_State + MenuElement_Action;
        public const string AssetMenuPrefix_Root_Mod_Controller_Transform = ComponentMenuPrefix_Root_Mod_Controller_Transform;
        public const string AssetMenuPrefix_Root_Mod_Controller_Transform_BoredAction = ComponentMenuPrefix_Root_Mod_Controller_Transform_BoredAction;

        public const string AssetMenuPrefix_Root_Common = AssetMenuPrefix_Root + MenuElement_Common;
        public const string AssetMenuPrefix_Root_Workshop = AssetMenuPrefix_Root + MenuElement_Workshop;
        public const string AssetMenuPrefix_Root_Persistent = AssetMenuPrefix_Root + MenuElement_Persistent;
        public const string AssetMenuPrefix_Root_BuiltIn = AssetMenuPrefix_Root + "BuiltIn/";
        public const string AssetMenuPrefix_Root_Feature = AssetMenuPrefix_Root + "Feature/";
    }
}