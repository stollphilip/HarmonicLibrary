using Microsoft.VisualStudio.TestTools.UnitTesting;
using HarmonicAnalysisLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using NoteInputLib;

namespace HarmonicAnalysisLib.Tests
{
    [TestClass()]
    public class ManagerTests
    {
        protected PitchPattern[] GenChords()
        {
            var chords = new PitchPattern[]
            {
                new PitchPattern
                {
                    Pitches = {
                        new Pitch(0, 0),
                        new Pitch(19, 1),
                        new Pitch(14, 2),
                        new Pitch(4, 4),
                    },
                },
                new PitchPattern
                {
                    Pitches = {
                        new Pitch(0, 0),
                        new Pitch(5, -1),
                        new Pitch(14, 2),
                        new Pitch(21, 3),
                    },
                },
                new PitchPattern
                {
                    Pitches = {
                        new Pitch(7, 1),
                        new Pitch(21, 3),
                        new Pitch(14, 2),
                        new Pitch(-1, 5),
                    },
                    //Pitches = {
                    //    new Pitch(22, -2),
                    //    new Pitch(7, 1),
                    //    new Pitch(0, 0),
                    //    new Pitch(14, 2),
                    //},
                },
            };
            return chords;
        }

        public void GenChords_ValidInput(Manager manager)
        {

        }
        public void PitchPatternToTones_Valid(Manager manager, PitchPattern[] chords)
        {
            Assert.IsTrue(manager.Groups.All(g => g.Type == GroupType.Tone));
            //Assert.IsTrue(manager.Groups.All(g => VectorConverter.IsNormalizedTone(g.Position)));
            Assert.AreEqual(manager.Groups.Count, chords.SelectMany(c => c.Pitches).Count());
            foreach (var group in manager.Groups)
            {
                Assert.IsTrue(group.Validate());
            }
        }
        public void Groups_Valid(Manager manager)
        {
            foreach (var group in manager.Groups)
            {
                Assert.IsTrue(group.Validate());
            }
            manager.ValidateGroups();
        }
        [TestMethod()]
        public void ProcessTest()
        {
            var manager = new Manager();
            manager.VerifyMethodOrder(0);
            var chords = GenChords();
            Assert.AreEqual(manager.Groups.Count, 0);

            manager.PitchPatternToTones(chords);
            PitchPatternToTones_Valid(manager, chords);
            manager.PrintGroups(GroupType.Tone);

            manager.TonesToGroups(chords);
            Groups_Valid(manager);
            manager.PrintGroups(GroupType.Group);

            manager.GroupsToVertGroups(chords);
            Groups_Valid(manager);
            manager.PrintGroups(GroupType.VerticalGroup);

            manager.TonesToFrames(chords);
            Groups_Valid(manager);
            manager.PrintGroups(GroupType.Frame);

            manager.GroupsToHorizGroups(chords);
            Groups_Valid(manager);
            manager.PrintGroups(GroupType.HorizontalGroup);

            manager.FrameToVerticalFrame(chords);
            Groups_Valid(manager);
            manager.PrintGroups(GroupType.VerticalFrame);

            manager.FrameToHorizFrame(chords);
            Groups_Valid(manager);
            manager.PrintGroups(GroupType.HorizontalFrame);

            manager.ChordShapes = FrameStructLib.InitFrameStructs(manager.Groups, chords);
            FrameStructLib.ValidateGroupMaps(manager.ChordShapes);

            // Dijkstra
            SegmentLib2.CallDijkstra(manager.ChordShapes, chords);

            // print navigation space
            FrameStructLib.ListChordShapes(manager.ChordShapes);

            // check whether any VerticalGroups contain aliases (they don't). HorizontalGroups do.
            CheckAliases(manager);

            SegmentLib.CallShortestPath_new(manager.ChordShapes, 35);
        }

        private static void CheckAliases(Manager manager)
        {
            foreach (var group in manager.Groups.Where(g => g.Type == GroupType./*HorizontalGroup*/VerticalGroup/*VerticalFrame*/))
            {
                var mirrors = manager.Groups.Where(g => g.ChordIndex == group.ChordIndex && g.Type == group.Type &&
                g.Children[0].Position == group.Children[1].Position &&
                g.Children[0].Group == group.Children[1].Group &&
                g.Children[1].Position == group.Children[0].Position &&
                g.Children[1].Group == group.Children[0].Group).ToList();
                if (mirrors.Count != 1)
                {
                }
                else if (mirrors.Count == 1)
                {
                    var mirror = mirrors[0];
                    if (group.Distance.Pairwise != mirror.Distance.Pairwise ||
                        group.Distance.Average != mirror.Distance.Average ||
                        group.Distance.Weighted != mirror.Distance.Weighted ||
                        group.Distance.CompoundPairwise != mirror.Distance.CompoundPairwise ||
                        group.Distance.CompoundAverage != mirror.Distance.CompoundAverage ||
                        group.Distance.CompoundWeighted != mirror.Distance.CompoundWeighted)
                    {
                    }
                }
            }
            foreach (var frameStruct in FrameStructLib.FrameStructIterator(manager.ChordShapes)
                .Where(fs => fs.GroupMap != null && fs.GroupMap.Type == GroupMapType.VerticalGroupMap &&
                fs.GroupMap.Aliases.Count == 2 &&
                fs.GroupMap.Aliases.All(chord => chord.Count == 1)))
            {
                frameStruct.GroupMap.AliasDistance(DistanceType.Pairwise);
            }
            foreach (var frameStruct in FrameStructLib.FrameStructIterator(manager.ChordShapes)
                .Where(fs => fs.GroupMap != null))
            {
                foreach (var e in Enum.GetValues(typeof(DistanceType)).Cast<DistanceType>())
                {
                    var s = $"{frameStruct.GroupMap.Distance.DistanceTypeToDouble(e):0}";
                    //Debug.Write($"{frameStruct.GroupMap.Distance.DistanceTypeToDouble(e):0}");
                }
                frameStruct.GroupMap.AliasDistance(DistanceType.Pairwise);
            }
        }

        [TestMethod()]
        public void StringInputTest()
        {
const string str =
@"
0 17 12 4
0 5 12 19
7 19 12 -1B
//
0 17 12 4
0 5 12 19
7 19 12 -1B
//NumberBase12
";
            var noteList = StringInput.ReadStringInput(str, Environment.NewLine);
            const string str2 = "0 17 12 4, 0 5 12 19, 7 19 12 -1B, //, 0 17 12 4, 0 5 12 19, 7 19 12 -1B, //NumberBase12";
            var noteList2 = StringInput.ReadStringInput(str2, ',');
        }
        [TestMethod()]
        public void InputFormatTest()
        {
            var noteList = FileInput.ReadFileInput(@"C:\Users\philip\source\repos\Projects\HarmonicLibrary\NoteInputLib\input.txt");
            PitchPattern[][] progressions = NoteListConverter.Convert(noteList);
            //List<List<List<(int height, int fifthHeight)>>> noteList_old = FileInput.UseFileInput_old(@"C:\Temp\harmonicanalaysislib.txt");
            //PitchPattern[][] progressions = noteList_old.Select(prog => (prog.Select(chord => new PitchPattern { Pitches = chord.Select(tone => new Pitch(tone.height, tone.fifthHeight)).Cast<IPitch>().ToList()}).ToArray())).ToArray();
            foreach (var chords in progressions)
            {
                var manager = new Manager();
                manager.VerifyMethodOrder(0);
                //var chords = GenChords();
                Assert.AreEqual(manager.Groups.Count, 0);

                manager.PitchPatternToTones(chords);
                PitchPatternToTones_Valid(manager, chords);
                //manager.PrintGroups(GroupType.Tone);

                manager.TonesToGroups(chords);
                Groups_Valid(manager);
                //manager.PrintGroups(GroupType.Group);

                manager.GroupsToVertGroups(chords);
                Groups_Valid(manager);
                //manager.PrintGroups(GroupType.VerticalGroup);

                manager.TonesToFrames(chords);
                Groups_Valid(manager);
                //manager.PrintGroups(GroupType.Frame);

                // this could use some more work
                //manager.FramesToPaths_experiment(chords);
                manager.VerifyMethodOrder(5);
                Groups_Valid(manager);

                manager.GroupsToHorizGroups(chords);
                Groups_Valid(manager);
                //manager.PrintGroups(GroupType.HorizontalGroup);

                manager.FrameToVerticalFrame(chords);
                Groups_Valid(manager);
                //manager.PrintGroups(GroupType.VerticalFrame);

                manager.FrameToHorizFrame(chords);
                Groups_Valid(manager);
                //manager.PrintGroups(GroupType.HorizontalFrame);

                //

                manager.ChordShapes = FrameStructLib.InitFrameStructs(manager.Groups, chords);
                FrameStructLib.ValidateGroupMaps(manager.ChordShapes);

                // print navigation space
                //FrameStructLib.ListChordShapes(manager.ChordShapes);
                SegmentLib2.CallDijkstra(manager.ChordShapes, chords);


                SegmentLib.CallShortestPath(manager.ChordShapes);
            }
        }
        private void DebugOutput(Manager manager)
        {
            // all the (y, z) values of the horizontal groups
            var list = new List<String>();
            var list2 = new List<String>();
            foreach (var group in manager.Groups.Where(g => g.Type == GroupType.HorizontalGroup))
            {
                list.Add($"{group.Position[1],2} {group.Position[2]} : {group.Children[0].Position[1]} {group.Children[0].Position[2]} : {group.Children[1].Position[1]} {group.Children[1].Position[2]}");
                list2.Add($"{group.Position[1],2} {group.Position[2]} : ");
            }
            var distinct = list.OrderBy(s => s).Distinct();
            var distinct2 = list2.OrderBy(s => s).Distinct();
            foreach (var item in distinct)
            {
                Debug.WriteLine($"{list.Count(s => s == item),3} : {item}");
            }
            foreach (var item in distinct2)
            {
                Debug.WriteLine($"{list.Count(s => item.StartsWith(s)),3} : {item}");
            }

            const int midY = 4;
            const int midZ = 3;
            int[][] array = new int[][] { new int[] { 0, 0, 0, 0, 0, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, }, new int[] { 0, 0, 0, 0, 0, 0, 0, }, };
            foreach (var group in manager.Groups.Where(g => g.Type == GroupType.HorizontalGroup))
            {
                var deltaY = group.Children[1].Position[1] - group.Children[0].Position[1] + midY;
                var deltaZ = group.Children[1].Position[2] - group.Children[0].Position[2] + midZ;
                if (0 <= deltaY && deltaY < array.Length && 0 < deltaZ && deltaZ < array[0].Length)
                {
                    array[deltaY][deltaZ]++;
                }
                else
                {

                }
            }
            Debug.Write($"{string.Empty,-8}");
            for (int y = 0; y < array.Length; ++y)
            {
                Debug.Write($"{y - midY,3}");
            }
            Debug.WriteLine(null);
            Debug.WriteLine(null);
            for (int z = array[0].Length - 1; z >= 0; --z)
            {
                Debug.Write($"{z - midZ,-8}");
                for (int y = 0; y < array.Length; ++y)
                {
                    Debug.Write($"{array[y][z],3}");
                }
                Debug.WriteLine(null);
            }
            //foreach (var row in array)
            //{
            //    foreach (var cell in row)
            //    {
            //        Debug.Write($"{cell,3}");
            //    }
            //}
        }
    }
}
