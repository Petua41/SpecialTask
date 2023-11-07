﻿using SpecialTask;
using SpecialTask.Drawing;
using SpecialTask.Infrastructure.Collections;
using SpecialTask.Infrastructure.Enums;
using static SpecialTask.Infrastructure.Extensoins.PointListExtensions;

namespace SpecialTaskTest
{
    public class MiscTest
    {
        [Test]
        public void MyMapConstructorTest()
        {
            List<KeyValuePair<string, int>> lst = new() { new("one", 1), new("two", 2) };
            Pairs<string, int> expected = new() { { "one", 1 }, { "two", 2 } };
            Pairs<string, int> actual = new(lst);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void MyMapCloneTest()
        {
            Pairs<string, int> expected = new() { { "one", 1 }, { "two", 2 } };
            Assert.That(expected.Clone(), Is.EqualTo(expected));
        }
        
        [Test]
        public void MyMapAdditionTest()
        {
            Pairs<string, int> firstMap = new() { { "one", 1 }, { "two", 2 } };
            Pairs<string, int> secondMap = new() { { "three", 3 }, { "one", 1 } };
            Pairs<string, int> expected = new() { { "one", 1 }, { "two", 2 }, { "three", 3 }, { "one", 1 } };
            Pairs<string, int> actual = firstMap + secondMap;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void PointsToStringTest()
        {
            List<Point> points = new() { new(10, 10), new(20, 30), new(40, 40) };
            string expected = "10 10, 20 30, 40 40";
            string actual = points.PointsToString();
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SplitHexValueTest()
        {
            (byte, byte, byte) expected = (255, 0, 130);
            uint hex = 0xFF0082;
            (byte, byte, byte) actual = ColorsController.SplitHexValue(hex);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}