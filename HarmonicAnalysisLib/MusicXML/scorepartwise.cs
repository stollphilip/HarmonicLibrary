using System.Numerics;

namespace HarmonicAnalysisLib.MusicXML;

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
[System.Xml.Serialization.XmlRootAttribute("score-partwise", Namespace = "", IsNullable = false)]
public class scorepartwise
{
    [System.Xml.Serialization.XmlElementAttribute("part")]
    public scorepartwisePart[] part;
}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public class scorepartwisePart
{
    [System.Xml.Serialization.XmlElementAttribute("measure")]
    public scorepartwisePartMeasure[] measure;
}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
public class scorepartwisePartMeasure
{
    //[System.Xml.Serialization.XmlElementAttribute("attributes", typeof(attributes))]
    //[System.Xml.Serialization.XmlElementAttribute("backup", typeof(backup))]
    //[System.Xml.Serialization.XmlElementAttribute("barline", typeof(barline))]
    //[System.Xml.Serialization.XmlElementAttribute("bookmark", typeof(bookmark))]
    //[System.Xml.Serialization.XmlElementAttribute("direction", typeof(direction))]
    //[System.Xml.Serialization.XmlElementAttribute("figured-bass", typeof(figuredbass))]
    //[System.Xml.Serialization.XmlElementAttribute("forward", typeof(forward))]
    //[System.Xml.Serialization.XmlElementAttribute("grouping", typeof(grouping))]
    //[System.Xml.Serialization.XmlElementAttribute("harmony", typeof(harmony))]
    //[System.Xml.Serialization.XmlElementAttribute("link", typeof(link))]
    //[System.Xml.Serialization.XmlElementAttribute("listening", typeof(listening))]
    [System.Xml.Serialization.XmlElementAttribute("note", typeof(note))]
    //[System.Xml.Serialization.XmlElementAttribute("print", typeof(print))]
    //[System.Xml.Serialization.XmlElementAttribute("sound", typeof(sound))]
    public object[] Items;


    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
    public string number;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
    public string text;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public yesno @implicit;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool implicitSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("non-controlling")]
    public yesno noncontrolling;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool noncontrollingSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal width;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool widthSpecified;
}

[System.SerializableAttribute()]
public class note
{

    [System.Xml.Serialization.XmlElementAttribute("chord", typeof(empty))]
    [System.Xml.Serialization.XmlElementAttribute("cue", typeof(empty))]
    [System.Xml.Serialization.XmlElementAttribute("duration", typeof(decimal))]
    [System.Xml.Serialization.XmlElementAttribute("grace", typeof(grace))]
    [System.Xml.Serialization.XmlElementAttribute("pitch", typeof(pitch))]
    [System.Xml.Serialization.XmlElementAttribute("rest", typeof(rest))]
    [System.Xml.Serialization.XmlElementAttribute("tie", typeof(tie))]
    [System.Xml.Serialization.XmlElementAttribute("unpitched", typeof(unpitched))]
    [System.Xml.Serialization.XmlElementAttribute("distance", typeof(distance))]
    [System.Xml.Serialization.XmlElementAttribute("tag", typeof(string/*tag*/))]
    [System.Xml.Serialization.XmlChoiceIdentifierAttribute("ItemsElementName")]
    public object[] Items;


    [System.Xml.Serialization.XmlElementAttribute("ItemsElementName")]
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public ItemsChoiceType1[] ItemsElementName;

    //[System.Xml.Serialization.XmlElementAttribute("instrument")]
    //public instrument[] instrument;

    //public formattedtext footnote;

    //public level level;

    public string voice;

    public notetype type;

    //[System.Xml.Serialization.XmlElementAttribute("dot")]
    //public emptyplacement[] dot;

    //public accidental accidental;

    //[System.Xml.Serialization.XmlElementAttribute("time-modification")]
    //public timemodification timemodification;

    public stem stem;

    //public notehead notehead;

    //[System.Xml.Serialization.XmlElementAttribute("notehead-text")]
    //public noteheadtext noteheadtext;

    [System.Xml.Serialization.XmlElementAttribute(DataType = "positiveInteger")]
    public string staff;

    //[System.Xml.Serialization.XmlElementAttribute("beam")]
    //public beam[] beam;

    //[System.Xml.Serialization.XmlElementAttribute("notations")]
    //public notations[] notations;

    //[System.Xml.Serialization.XmlElementAttribute("lyric")]
    //public lyric[] lyric;

    //public play play;

    //[System.Xml.Serialization.XmlArrayItemAttribute(typeof(assess), IsNullable = false)]
    //[System.Xml.Serialization.XmlArrayItemAttribute("other-listen", typeof(otherlistening), IsNullable = false)]
    //[System.Xml.Serialization.XmlArrayItemAttribute(typeof(wait), IsNullable = false)]
    //public object[] listen;

    //[System.Xml.Serialization.XmlAttributeAttribute("default-x")]
    //public decimal defaultx;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool defaultxSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute("default-y")]
    //public decimal defaulty;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool defaultySpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute("relative-x")]
    //public decimal relativex;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool relativexSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute("relative-y")]
    //public decimal relativey;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool relativeySpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute("font-family", DataType = "token")]
    //public string fontfamily;

    //[System.Xml.Serialization.XmlAttributeAttribute("font-style")]
    //public fontstyle fontstyle;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool fontstyleSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute("font-size")]
    //public string fontsize;

    //[System.Xml.Serialization.XmlAttributeAttribute("font-weight")]
    //public fontweight fontweight;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool fontweightSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
    //public string color;

    //[System.Xml.Serialization.XmlAttributeAttribute("print-dot")]
    //public yesno printdot;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool printdotSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute("print-lyric")]
    //public yesno printlyric;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool printlyricSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute("print-leger")]
    //public yesno printleger;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool printlegerSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute()]
    //public decimal dynamics;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool dynamicsSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute("end-dynamics")]
    //public decimal enddynamics;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool enddynamicsSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute()]
    //public decimal attack;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool attackSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute()]
    //public decimal release;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool releaseSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute("time-only", DataType = "token")]
    //public string timeonly;

    //[System.Xml.Serialization.XmlAttributeAttribute()]
    //public yesno pizzicato;

    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool pizzicatoSpecified;

    //[System.Xml.Serialization.XmlAttributeAttribute(DataType = "ID")]
    //public string id;
}

[System.SerializableAttribute()]
public class grace
{

}

[System.SerializableAttribute()]
public class pitch
{

    public step step;


    public decimal alter;


    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool alterSpecified;


    [System.Xml.Serialization.XmlElementAttribute(DataType = "integer")]
    public string octave;

    public string position;
}

[System.SerializableAttribute()]
public class rest
{

}

[System.SerializableAttribute()]
public class tie
{

}

[System.SerializableAttribute()]
public class unpitched
{
}

[System.Xml.Serialization.XmlIncludeAttribute(typeof(release))]
[System.SerializableAttribute()]
public class empty
{

}

[System.SerializableAttribute()]
public partial class release : empty
{

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute()]
    public decimal offset;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool offsetSpecified;
}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "note-type")]
public class notetype
{
    //[System.Xml.Serialization.XmlAttributeAttribute()]
    //public symbolsize size;

    ///// <remarks/>
    //[System.Xml.Serialization.XmlIgnoreAttribute()]
    //public bool sizeSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public notetypevalue Value;
}

[System.SerializableAttribute()]
public partial class stem
{

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("default-x")]
    public decimal defaultx;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool defaultxSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("default-y")]
    public decimal defaulty;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool defaultySpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("relative-x")]
    public decimal relativex;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool relativexSpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute("relative-y")]
    public decimal relativey;

    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool relativeySpecified;

    /// <remarks/>
    [System.Xml.Serialization.XmlAttributeAttribute(DataType = "token")]
    public string color;

    /// <remarks/>
    [System.Xml.Serialization.XmlTextAttribute()]
    public stemvalue Value;
}

[System.SerializableAttribute()]
public class distance
{
    [System.Xml.Serialization.XmlAttributeAttribute("weighted")]
    public double Weighted;

    [System.Xml.Serialization.XmlAttributeAttribute("compound-weighted")]
    public double CompoundWeighted;
}

[System.SerializableAttribute()]
public class tag
{
    [System.Xml.Serialization.XmlAttributeAttribute("value")]
    public string Value;
    [System.Xml.Serialization.XmlAttributeAttribute("level")]
    public int Level;
}
[System.SerializableAttribute()]
public enum step
{


    A,


    B,


    C,


    D,


    E,


    F,


    G,
}

[System.SerializableAttribute()]
public enum ItemsChoiceType1
{


    chord,


    cue,


    duration,


    grace,


    pitch,


    rest,


    tie,


    unpitched,

    distance,

    tag,
}
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "note-type-value")]
public enum notetypevalue
{

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("1024th")]
    Item1024th,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("512th")]
    Item512th,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("256th")]
    Item256th,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("128th")]
    Item128th,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("64th")]
    Item64th,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("32nd")]
    Item32nd,

    /// <remarks/>
    [System.Xml.Serialization.XmlEnumAttribute("16th")]
    Item16th,

    /// <remarks/>
    eighth,

    /// <remarks/>
    quarter,

    /// <remarks/>
    half,

    /// <remarks/>
    whole,

    /// <remarks/>
    breve,

    /// <remarks/>
    @long,

    /// <remarks/>
    maxima,
}

[System.SerializableAttribute()]
[System.Xml.Serialization.XmlTypeAttribute(TypeName = "stem-value")]
public enum stemvalue
{

    /// <remarks/>
    down,

    /// <remarks/>
    up,

    /// <remarks/>
    @double,

    /// <remarks/>
    none,
}

[System.Xml.Serialization.XmlTypeAttribute(TypeName = "yes-no")]
public enum yesno
{

    /// <remarks/>
    yes,

    /// <remarks/>
    no,
}
