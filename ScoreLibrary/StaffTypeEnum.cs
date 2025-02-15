using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScoreLibrary
{
    public enum StaffTypeEnum
    {
        None = 0,
        PitchHeight,
        XYZ,
        YZ,
        Y,
    }
    public enum XYZStaffLayout
    {
        None,
        Planar,
        Shifted,
    }
    public enum StemDirection
    {
        Up,
        Down,
    }
    public enum ExtendDirection
    {
        None,
        Left,
        Right,
    }
    [Flags]
    public enum ObjectMappings
    {
        //Tip: myEnum.HasFlag(flagValue)
        None = 0,
        Frame_in_1_col = 1,

        HorizFrame_in_1_col = 2,
        HorizFrame_2_Frames_in_1_col = 4,
        HorizFrame_2_Frames_in_2_cols = 8,
        HorizFrame_N_DualGroups_in_2_Cols = 16,
        HorizFrame_N_DualGroups_in_2N_Cols = 32,
        HorizFrame_N_DualGroups_in_2N_Cols_alternating = 64,

        VertFrame_in_1_col = 128,
        VertFrame_2_Frames_in_1_col = 256,
        VertFrame_2_Frames_in_2_cols = 512,
        VertFrame_N_DualGroups_in_2_Cols = 1024,
        VertFrame_N_DualGroups_in_2N_Cols = 2048,
        VertFrame_N_DualGroups_in_2N_Cols_alternating = 4096,

        Group_2_Child_in_1_col = 8192,
        Group_2_Child_in_2_col = 16384,
        HorizGroup_2_Child_in_1_col = 32768,
        HorizGroup_2_Child_in_2_col = 65536,
        Segment_in_1_col = 131072,
    }
    public enum Side
    {
        Left,
        Right,
        Top,
        Bottom,
    }
}
