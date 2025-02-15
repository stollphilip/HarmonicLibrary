using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using HarmonicAnalysisCommonLib;
using ScoreLibrary.Base;
using TextMetricLib;

namespace ScoreLibrary.PitchHeightStaff
{
    public class ChordStruct : ChordStructBase
    {
        public override LocationStruct Translate(Vector<int> position)
        {
            // TODO: implement ChordStruct Translate
            var x = position[0];
            var y = position[1];
            var z = position[2];
            return new LocationStruct(-(y + 4 * z), 0, 2, 2);

        }
        public override LocationStruct Translate(ToneLite pitch)
        {
            var stepStruct = new StepStruct(pitch);
            return new LocationStruct(stepStruct.row, 0, 2, 2);
        }
        public override LocationStruct Translate(ToneLite pitch, int rowSpan, int columnSpan)
        {
            var stepStruct = new StepStruct(pitch);
            return new LocationStruct(stepStruct.row, 0, rowSpan, columnSpan);
        }
        public override void Scale(ref LocationStruct location)
        {
            const int rowScaleFactor = 1;
            const int columnScaleFactor = 2;
            location.Row *= rowScaleFactor;
            location.Column *= columnScaleFactor;
        }
        public void NEWCODEBELOW() {}
        private StemDirection StemDirection
        {
            get; set;
        }
        public readonly int[] staffs = new int[] { /*1, 2*/0, 1 };
        public int StepToStaff(StepStruct stepStruct) => (stepStruct.row <= StepStruct.TurningPoint) ? staffs[0] : staffs[1];
        /*
        /// <summary>
        /// Get the location of an element with a delta
        /// </summary>
        private LocationStruct GetLocation(ElementStructBase t, int delta = 0)
        {
            return new LocationStruct(t.Location.Row, t.Location.Column + delta, t.Location.RowSpan, t.Location.ColumnSpan);
        }
        /// <summary>
        /// Get the next available Grid location for an element, implementing collision avoidance
        /// </summary>
        /// <param name="elem">the element whose location is being requested</param>
        /// <param name="stemDirection">stem direction</param>
        private LocationStruct NextAvailableLocation(ElementStructBase elem, StemDirection stemDirection)
        {
            var tag = elem.Tag as string;
            var delta = 0;
            switch (tag)
            {
                case "note":
                case "clef":
                    while (elementStructs.Where(x => x.Tag == tag && x.IsLaidOut).Any(x => LocationStruct.IsIntersect(GetLocation(elem, delta), x.Location)))
                    {
                        if (stemDirection == StemDirection.Up)
                        {
                            ++delta;
                        }
                        else
                        {
                            --delta;
                        }
                    }
                    return GetLocation(elem, delta);
                case "dot":
                    //TODO: implement
                    return GetLocation(elem, delta);
                case "alter":
                    while (elementStructs.Where(x => (x.Tag == "note" || x.Tag == "alter") && x.IsLaidOut).Any(x => LocationStruct.IsIntersect(GetLocation(elem, delta), x.Location)))
                    {
                        --delta;
                    }
                    return GetLocation(elem, delta);
                default:
                    throw new InvalidOperationException();
            }
        }
        */
        /// <summary>
        /// Calculate stem direction
        /// </summary>
        public (StemDirection stemDirection, int stemBottomStep, int stemTopStep) CalculateStem(int staff)
        {
            //Take the mean of the the highest and lowest scale degrees on the staff
            //if the mean is >= the middle staff line, point the step down, otherwise point the stem up
            if (elementStructs.Count(e => e.Staff == staff && e.Tag == "note") == 0)
            {
                // no notes on the staff
                return (StemDirection.Up, 0, 0);
            }
            var maxStep = elementStructs.Where(e => e.Staff == staff && e.Tag == "note").Max(e => e.StepToRow.row);
            var minStep = elementStructs.Where(e => e.Staff == staff && e.Tag == "note").Min(e => e.StepToRow.row);
            var mean = (double)(maxStep + minStep) / 2.0;
            var middleLine = 0;

            switch (staff)
            {
                case 0:
                    middleLine = StepStruct.StepToRow.Single(x => x.step == "B4").row;
                    break;
                case 1:
                    middleLine = StepStruct.StepToRow.Single(x => x.step == "D3").row;
                    break;
            }

            //Calculate stem length
            //	for stem pointed up, the end of the stem is one octave above the lowest scale degree on the staff, or the middle line, whichever is greater
            //	for stem pointed down, the end of the stem is one octave below the lowest scale degree on the staff, or the middle line, whichever is less
            if (mean <= middleLine)
                return (StemDirection.Down, Math.Min(minStep - 7, middleLine), maxStep);
            else
                return (StemDirection.Up, minStep, Math.Max(maxStep + 7, middleLine));
        }
        /// <summary>
        /// Layout notes on the staff, implementing collision avoidance
        /// </summary>
        public override void LayoutNotes()
        {
            foreach (var staff in this.staffs)
            {
                (StemDirection stemDirection, int _, int _) = this.CalculateStem(staff);
                base.LayoutNotes(staff, stemDirection);
            }
        }
        /// <summary>
        /// Layout accidentals on the staff, implementing collision avoidance
        /// </summary>
        public void LayoutAccidental()
        {
            foreach (var staff in this.staffs)
            {
                LayoutAccidental(staff);
            }
        }
        public void LayoutAccidental(int staff)
        {
            // accidentals ordered by row
            var elements = elementStructs.Where(e => e.Staff == staff && e.Tag == "alter").OrderBy(e => e.StepToRow.row).ToList();
            Debug.Assert(elements.All(e => !e.IsLaidOut));
            while (elements.Any(e => !e.IsLaidOut))
            {
                // get next range of accidentals
                var list = new List<ElementStructBase>();
                foreach (var element in elements.Where(l => !l.IsLaidOut))
                {
                    if (list.Count == 0 || list[0].Location.Row - element.Location.Row <= 6)
                    {
                        list.Add(element);
                    }
                }
                // range of accidentals in alternating order
                var alternatingList = new List<ElementStructBase>();
                int count = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    alternatingList.Add(list[i]);
                    count++;
                    if (count == list.Count)
                    {
                        break;
                    }
                    alternatingList.Add(list[list.Count - 1 - i]);
                    count++;
                    if (count == list.Count)
                    {
                        break;
                    }
                }
                foreach (var element in alternatingList)
                {
                    // StemDirection doesn't matter here
                    LocationStruct p = NextAvailableLocation(element, StemDirection.Down);
                    element.Location = new LocationStruct(p.Row, p.Column, p.RowSpan, p.ColumnSpan);
                    element.IsLaidOut = true;
                }
            }
        }
        /// <summary>
        /// Add ledger lines to the staff
        /// </summary>
        public void LayoutLedger()
        {
            foreach (var staff in this.staffs)
            {
                LayoutLedger(staff);
            }
        }
        public void LayoutLedger(int staff_)
        {
            // top and bottom lines of staff
            List<(int staff, int topStep, int bottomStep)> datas = new List<(int staff, int topStep, int bottomStep)> {
                (0, StepStruct.StepToRow.Single(x => x.step == "F5").row, StepStruct.StepToRow.Single(x => x.step == "E4").row),
                (1, StepStruct.StepToRow.Single(x => x.step == "A3").row, StepStruct.StepToRow.Single(x => x.step == "G2").row),
            };
            var frameworkElementsAdded = new List<FrameworkElement>();
            // foreach staff
            foreach (var datum in datas)
            {
                (int staff, int topLine, int bottomLine) = datum;
                if (elementStructs.Count(e => e.Tag == "note" && e.Staff == staff) == 0)
                    continue;

                // stem direction for the staff
                (StemDirection stemDirection, _/*int stemBottomStep*/, _/*int stemTopStep*/) = CalculateStem(staff);

                // highest step above the staff
                var min = Math.Min(topLine, elementStructs.Where(e => e.Tag == "note" && e.Staff == staff).Min(e => e.StepToRow.row));
                // lowest step below the staff
                var max = Math.Max(bottomLine, elementStructs.Where(e => e.Tag == "note" && e.Staff == staff).Max(e => e.StepToRow.row));
                // range of steps above the staff
                var topRange = Enumerable.Range(min, topLine - min).ToList();
                // range of steps below the staff
                var bottomRange = Enumerable.Range(bottomLine + 1, max - bottomLine).Reverse().ToList();
                var ranges = new List<List<int>> { topRange, bottomRange };
                // center column of notes spanning both staffs
                int center = GetCenterColumn();
                // extend the ledger lines to the left or right. this is a sticky flag. once it is set, all subsquent ledger lines will extend in the same direction.
                var extendDir = ExtendDirection.None;
                var textMetric = TextMetric.Data.SingleOrDefault(x => x.Text == "-");
                var textMetricWide = TextMetric.Data.SingleOrDefault(x => x.Text == "_-");
                bool even;

                string evenString;
                string noteString;
                string ledgeString;
                foreach (var range in ranges)
                {
                    foreach (int row in range)
                    {
                        // get element, if any
                        var note = elementStructs.FirstOrDefault(e => e.Tag == "note" && e.Staff == staff && e.StepToRow.row == row);
                        if (note != null)
                        {
                            if (note.Location.Column < center)
                            {
                                if (extendDir == ExtendDirection.Right) throw new Exception();
                                extendDir = ExtendDirection.Left;
                            }
                            else if (note.Location.Column > center)
                            {
                                if (extendDir == ExtendDirection.Left) throw new Exception();
                                extendDir = ExtendDirection.Right;
                            }
                        }
                        even = Math.Abs(row - topLine) % 2 == 0;
                        if (even)
                        {
                            int column = extendDir == ExtendDirection.Left ? center + textMetricWide.Column : center + textMetric.Column;
                            int columnSpan = extendDir == ExtendDirection.None ? textMetric.ColumnSpan : textMetricWide.ColumnSpan;
                            var tm = extendDir == ExtendDirection.None ? textMetric : textMetricWide;
                            var ledger = elementStructs.SingleOrDefault(e => e.Tag == "ledger" && e.Staff == staff && e.StepToRow.row == row);
                            if (ledger == null)
                            {
                                var rectangle = new Rectangle
                                {
                                    Width = Zoom.GridRowHeightColumnSize * (tm.ColumnSpan),
                                    Height = Zoom.RectangleHeight,
                                    Fill = new SolidColorBrush(Colors.Black),
                                    HorizontalAlignment = HorizontalAlignment.Left,
                                    Margin = new Thickness(tm.MarginLeft * Zoom.ZoomFactor, 0, 0, 0),
                                    Tag = "ledger",
                                };
                                var stepToRow = StepStruct.StepToRow.Single(x => x.row == row);
                                var positionStruct = new PositionStruct { StepStruct = stepToRow };
                                var locationStruct = new LocationStruct(row + tm.Row, column, tm.RowSpan, columnSpan);
                                ledger = new ElementStructBase(rectangle, locationStruct, positionStruct, "ledger", staff);
                                frameworkElementsAdded.Add(ledger.Element);
                                elementStructs.Add(ledger);
                            }
                            else
                            {
                                ledger.Element.Width = Zoom.GridRowHeightColumnSize * (tm.ColumnSpan);
                            }
                            ledger.Location = new LocationStruct(row + tm.Row, column, tm.RowSpan, columnSpan);
                            ledgeString = ledger.ToString();
                        }
                        else ledgeString = string.Empty;

                        noteString = note != null ? "note" : string.Empty;
                        evenString = even ? "even" : string.Empty;
                        //Debug.WriteLine($"{row} {evenString,4} {noteString,4} {ledgeString}");
                    }
                }
            }
        }
        public int GetCenterColumn()
        {
            if (this.StemDirection == StemDirection.Up)
            {
                return elementStructs.Where(e => e.Tag == "note").OrderByDescending(e => e.Location.Row).First().Location.Column;
            }
            else
            {
                return elementStructs.Where(e => e.Tag == "note").OrderBy(e => e.Location.Row).First().Location.Column;
            }
        }
        /// <summary>
        /// Update the columns so that the minimum column is 0
        /// </summary>
        /// <remarks>since collision avoidance moved elements to the left some elements have negative columns</remarks>
        public void UpdateColumn()
        {
            if (elementStructs.Count == 0)
            {
                return;
            }   
            const int margin = 2;
            var minColumn = elementStructs.Where(e => e.Tag == "note" || e.Tag == "alter" || e.Tag == "ledger").Min(e => e.Location.Column);
            if (minColumn < 0)
            {
                foreach (var element in elementStructs.Where(e => e.Tag == "note" || e.Tag == "alter" || e.Tag == "ledger" || e.Tag == "border"))
                {
                    element.Location.Column += -minColumn /*+ margin*/;
                }
            }
        }
    }
}
