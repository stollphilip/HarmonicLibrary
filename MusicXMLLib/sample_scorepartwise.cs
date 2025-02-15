using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace MusicXMLLib;
public class sample_scorepartwise
{
    public const string FileNameIn = @"C:\Users\philip\Downloads\Piano.xml";
    public const string FileNameOut = @"C:\Users\philip\Downloads\Piano.mod.xml";
    public static scorepartwise Read(string path = FileNameIn)
    {
        scorepartwise ScorePartwise = null;
        XmlSerializer serializer = new XmlSerializer(typeof(scorepartwise));
        using (StreamReader reader = new StreamReader(path, Encoding.UTF8))
        {
            ScorePartwise = (scorepartwise)serializer.Deserialize(reader);
        }
        return ScorePartwise;
    }
    public static void Write(scorepartwise score)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(scorepartwise));
        using (FileStream fileStream = new FileStream(FileNameOut, FileMode.Create))
        {
            using (StreamWriter reader = new StreamWriter(fileStream, Encoding.UTF8))
            {
                serializer.Serialize(reader, score);
            }
        }
    }
    public static IEnumerable<List<note>> ChordIterator(scorepartwise score)
    {
        var list = new List<note>();
        foreach (var note in score.part[0].measure[0].Items.OfType<note>())
        {
            if (note.ItemsElementName.Count(i => i == ItemsChoiceType1.chord) == 0)
            {
                // start new chord
                if (list.Any())
                {
                    yield return list;
                }
                list = new List<note> { note };
            }
            else
            {
                // continue chord
                list.Add(note);
            }
        }
        if (list.Any())
        {
            yield return list;
        }
    }
    public static void AddNote(scorepartwise score, empty chord, pitch pitch, distance distance, string tag, string voice, notetype type, stem stem, string staff)
    {
        //var pitch = new pitch
        //{
        //    alter = alter,
        //    alterSpecified = alterSpecified,
        //    octave = octave,
        //    position = position,
        //    step = step
        //};
        var note = new note
        {
            voice = voice,
            type = type,
            stem = stem,
            staff = staff,

        };
        AddNote(score, chord, pitch, distance, tag, note);
        //measure.Items
    }

    private static void AddNote(scorepartwise score, empty chord, pitch pitch, distance distance, string tag, note note)
    {
        var measure = score.part[0].measure[0];

        measure.AppendToMeasure(note);

        if (chord != null)
        {
            note.AppendToNote(chord, ItemsChoiceType1.chord);
        }
        note.AppendToNote(pitch, ItemsChoiceType1.pitch);
        if (distance != null)
        {
            note.AppendToNote(distance, ItemsChoiceType1.distance);
        }
        if (tag != null)
        {
            note.AppendToNote(tag, ItemsChoiceType1.tag);
        }
    }
}
