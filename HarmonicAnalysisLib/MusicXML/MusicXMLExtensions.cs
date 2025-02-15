using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace HarmonicAnalysisLib.MusicXML;
public static class MusicXMLExtensions
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="note">note</param>
    /// <param name="obj">pitch or chord</param>
    public static void AppendToNote(this note note, object obj, ItemsChoiceType1 itemsChoiceType1)
    {
        List<object> objs;
        List<ItemsChoiceType1> strs;
        if (note.Items == null)
        {
            objs = new List<object> { obj };
            strs = new List<ItemsChoiceType1> { itemsChoiceType1 };
        }
        else
        {
            objs = note.Items.ToList();
            strs = note.ItemsElementName.ToList();
            objs.Add(obj);
            strs.Add(itemsChoiceType1);
        }
        note.Items = objs.ToArray();
        note.ItemsElementName = strs.ToArray();
    }
    public static void AppendToMeasure(this scorepartwisePartMeasure measure, object obj)
    {
        List<object> objs;
        if (measure.Items == null)
        {
            objs = new List<object> { obj };
        }
        else
        {
            objs = measure.Items.ToList();
            objs.Add(obj);
        }
        measure.Items = objs.ToArray();
    }
    public static pitch pitch_from_note(this note note)
    {
        return note.Items.OfType<pitch>().SingleOrDefault();
    }
    public static distance distance_from_note(this note note)
    {
        return note.Items.OfType<distance>().SingleOrDefault();
    }
    public static string tag_from_note(this note note)
    {
        var indexOf = note.ItemsElementName.ToList().IndexOf(ItemsChoiceType1.tag);
        return note.Items[indexOf] as string;
    }
    public static Vector<int> ParsePosition(this string s)
    {
        var split = s.Split(' ');
        var x = int.Parse(split[0]);
        var y = int.Parse(split[1]);
        var z = int.Parse(split[2]);
        return new Vector<int>(new int[] { x, y, z, 0, 0, 0, 0, 0 });
    }
    public static int step_to_int(this step s)
    {
        switch (s)
        {
            case step.A:
                return 5;
            case step.B:
                return 6;
            case step.C:
                return 0;
            case step.D:
                return 1;
            case step.E:
                return 2;
            case step.F:
                return 3;
            case step.G:
                return 4;
            default: throw new InvalidOperationException();
        }
    }
        //public static object[] AppendToArray(this object[] Items, object o)
        //{
        //    var list = Items.ToList();
        //    list.Add(o);
        //    return list.ToArray();
        //}
        //public static ItemsChoiceType1[] AppendItemsElementName(this ItemsChoiceType1[] ItemsElementName, ItemsChoiceType1 itemChoiceType1)
        //{
        //    var list = ItemsElementName.ToList();
        //    list.Add(itemChoiceType1);
        //    return list.ToArray();
        //}
}