using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using ScoreLibrary.Base;
using HarmonicAnalysisCommonLib;
using TextMetricLib;
using HarmonicAnalysisCommonLib.Quarantine;
using ScoreLibrary.Parameter;

namespace ScoreLibrary.XYZStaff
{
    public class StaffProgressionStruct : StaffProgressionStructBase
    {
        public override StaffTypeEnum StaffType => StaffTypeEnum.XYZ;
        public StaffProgressionStruct(ProgressionParam staffParameter) : base(staffParameter)
        {

        }
        public override void AddChord(ColumnData columnData)
        {
            var chord = columnData.notes;
            var chordStruct = new XYZStaff.ChordStruct();
            Add(chordStruct);
            var tm = GetTextMetric(staffParameter.ShowNoteAs);
            foreach (var note in chord)
            {
                var pitch_pos = note.PositionFinal;
                var stepStruct = new StepStruct(note);
                var locationStruct = chordStruct.Translate(pitch_pos, staffParameter.XYZStaffLayout, tm.RowSpan, tm.ColumnSpan);
                chordStruct.Scale(ref locationStruct);
                PositionStruct Position = new PositionStruct
                {
                    Position = pitch_pos,
                    StepStruct = stepStruct,
                };
                TextBlock text;
                switch (staffParameter.ShowNoteAs)
                {
                    case ShowNoteAs.NumberBase10:
                    case ShowNoteAs.NumberBase12:
                    case ShowNoteAs.Scientific:
                        text = new TextBlock
                        {
                            // TODO: this line neccesitated adding reference to NoteInputLib
                            Text = note.ToneLiteToString(staffParameter.ShowNoteAs),
                            FontSize = tm.FontSize * fontScaleFactor,
                            Foreground = Brushes.Black,
                            HorizontalAlignment = HorizontalAlignment.Left,
                            Margin = new Thickness(tm.MarginLeft * fontScaleFactor, tm.MarginTop * fontScaleFactor - margin, 0, 0),
                            //Margin = new Thickness(Zoom.GridRowHeightColumnSize/*tm.MarginLeft * fontScaleFactor*/, 0/*tm.MarginTop * fontScaleFactor - margin*/, 0, 0),
                            Tag = "note",
                        };
                        break;
                    case ShowNoteAs.Symbol:
                    default:
                        text = new TextBlock
                        {
                            Text = tm.Text,
                            FontSize = tm.FontSize * fontScaleFactor,
                            Foreground = chordStruct.MapColor(pitch_pos, staffParameter.XYZStaffLayout),
                            Margin = new Thickness(tm.MarginLeft * fontScaleFactor, tm.MarginTop * fontScaleFactor - margin, 0, 0),
                            Tag = "note",
                        };
                        break;
                }
                /*
                var text = new TextBlock
                {
                    Text = tm.Text,
                    FontSize = tm.FontSize * fontScaleFactor,
                    Foreground = Brushes.Black,
                    Margin = new Thickness(tm.MarginLeft * fontScaleFactor, tm.MarginTop * fontScaleFactor - margin, 0, 0),
                    Tag = "note",
                };
                */
                var elementStruct = new ElementStructBase(text, locationStruct, Position, "note", 0, note.Hidden);
                // TODO: this is a temporary fix
                // TODO: fix. for Planar layout, two notes in the same location are expected
                if (chordStruct.FindElement(locationStruct) != null)
                {
                    continue;
                }
                chordStruct.Add(elementStruct);
            }
            // note collision avoidance
            chordStruct.LayoutNotes();
            // "border"
            foreach (var notes in columnData.BorderData.Select(bd => bd.notes))
            {
                // Tag: ChordWithoutNotes - do not remove this comment
                if (notes.Count(n => !n.Hidden) == 0)
                {
                    continue;
                }
                // the locations of the notes to be enclosed within the border
                var locationStructs = new List<LocationStruct>();
                foreach (var note in notes)
                {
                    var pitch_pos = note.PositionFinal;
                    var locationStruct = chordStruct.Translate(pitch_pos, staffParameter.XYZStaffLayout, tm.RowSpan, tm.ColumnSpan);
                    chordStruct.Scale(ref locationStruct);
                    locationStructs.Add(locationStruct);
                }
                // get the laid out locations
                if (locationStructs.Select(l => chordStruct.FindElement(l)).Any(l => l == null))
                {
                    throw new Exception("Element not found");
                }
                var layoutLocationStructs = locationStructs.Select(l => chordStruct.FindElement(l)).Select(e => e.Location).ToList();
                // Grid location of the border
                var locationStruct2 = new LocationStruct
                (
                    layoutLocationStructs.Min(l => l.Row),
                    layoutLocationStructs.Min(l => l.Column),
                    layoutLocationStructs.Max(l => l.Row + l.RowSpan) - layoutLocationStructs.Min(l => l.Row),
                    layoutLocationStructs.Max(l => l.Column + l.ColumnSpan) - layoutLocationStructs.Min(l => l.Column)
                );

                var scale = Zoom.GridRowHeightColumnSize;
                var points = layoutLocationStructs.Select(l => new Point((l.Column - locationStruct2.Column) * scale, (l.Row - locationStruct2.Row) * scale)).ToList();
                // 2 * scale is the width of a note
                var hullStruct = new HullStructBase(points, 2 * scale);
                var path = Algorithm.RoundedCorners.CreatePath(hullStruct.Hull, scale);
                // uncomment to debug
                var border = new Border
                {
                    Width = locationStruct2.ColumnSpan * scale,
                    Height = locationStruct2.RowSpan * scale,
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.Black,
                    BorderThickness = new Thickness(1),
                    // so that notes are centered on the staff line
                    Margin = new Thickness(0, -scale, 0, 0),
                    Tag = "border",
                };
                var elementStruct2 = new ElementStructBase(/*border*/path, locationStruct2, new PositionStruct(), "border")
                {
                    Hull = hullStruct,
                };
                if (staffParameter.ShowHull)
                {
                    chordStruct.Add(elementStruct2);
                }
                // Eventaully, distances will be inside the border. For now, distances are outside the rectangular location of the border.
                var distanceType = staffParameter.DistanceType;
                // the following line serves a purpose
                var showDistance = staffParameter.ShowDistance && distanceType != DistanceType.None;
                if (showDistance)
                {
                    double distance = columnData.BorderData[0].distance.DistanceTypeToDouble(distanceType);
                    var l = locationStruct2;
                    var tmd = GetTextMetric("1");
                    LocationStruct loc;
                    var alignment = staffParameter.ContentAlignment;
                    switch (alignment)
                    {
                        case ContentAlignment.TopLeft:
                            loc = new LocationStruct(l.Row - 2, l.Column, tmd.RowSpan, tmd.ColumnSpan);
                            break;
                        case ContentAlignment.TopCenter:
                            loc = new LocationStruct(l.Row - 2, l.Column + l.ColumnSpan / 2, tmd.RowSpan, tmd.ColumnSpan);
                            break;
                        case ContentAlignment.TopRight:
                            loc = new LocationStruct(l.Row - 2, l.Column + l.ColumnSpan, tmd.RowSpan, tmd.ColumnSpan);
                            break;
                        case ContentAlignment.MiddleLeft:
                            loc = new LocationStruct(l.Row + l.RowSpan / 2, l.Column - 2, tmd.RowSpan, tmd.ColumnSpan);
                            break;
                        case ContentAlignment.MiddleCenter:
                            // MiddleCenter doesn't make sense
                            loc = new LocationStruct(l.Row + l.RowSpan / 2, l.Column + l.ColumnSpan + 2, tmd.RowSpan, tmd.ColumnSpan);
                            break;
                        case ContentAlignment.MiddleRight:
                            loc = new LocationStruct(l.Row + l.RowSpan / 2, l.Column + l.ColumnSpan + 2, tmd.RowSpan, tmd.ColumnSpan);
                            break;
                        case ContentAlignment.BottomLeft:
                            loc = new LocationStruct(l.Row + l.RowSpan + 2, l.Column, tmd.RowSpan, tmd.ColumnSpan);
                            break;
                        case ContentAlignment.BottomCenter:
                            loc = new LocationStruct(l.Row + l.RowSpan + 2, l.Column + l.ColumnSpan / 2, tmd.RowSpan, tmd.ColumnSpan);
                            break;
                        case ContentAlignment.BottomRight:
                            loc = new LocationStruct(l.Row + l.RowSpan + 2, l.Column + l.ColumnSpan, tmd.RowSpan, tmd.ColumnSpan);
                            break;
                        default:
                            throw new InvalidOperationException();
                    }
                    var text = new TextBlock
                    {
                        Text = $"{distance:0.}",
                        FontSize = tmd.FontSize * fontScaleFactor,
                        Foreground = Brushes.Black,
                        HorizontalAlignment = HorizontalAlignment.Left,
                        Margin = new Thickness(tmd.MarginLeft * fontScaleFactor, tmd.MarginTop * fontScaleFactor - margin, 0, 0),
                        Tag = "note",
                    };
                    var elementStruct = new ElementStructBase(text, loc, new PositionStruct(), "distance");
                    chordStruct.Add(elementStruct);
                    //
                }
                // hull collision avoidance
                chordStruct.LayoutHulls();
                // minimum Column of the elements is 0
                chordStruct.Normalize();
                // update chord Location
                //chordStruct.UpdateLocation();
            }
        }
        public override bool ShowStaff => staffParameter.ShowXYZ;
    }
}
