using SpecialTask;

namespace SpecialTaskTest
{
    public class MiscTest
    {
        [Test]
        public void MyMapConstructorTest()
        {
            List<KeyValuePair<string, int>> lst = new() { new("one", 1), new("two", 2) };
            MyMap<string, int> expected = new() { { "one", 1 }, { "two", 2 } };
            MyMap<string, int> actual = new(lst);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void MyMapCloneTest()
        {
            MyMap<string, int> expected = new() { { "one", 1 }, { "two", 2 } };
            Assert.That(expected.Clone(), Is.EqualTo(expected));
        }
        
        [Test]
        public void MyMapAdditionTest()
        {
            MyMap<string, int> firstMap = new() { { "one", 1 }, { "two", 2 } };
            MyMap<string, int> secondMap = new() { { "three", 3 }, { "one", 1 } };
            MyMap<string, int> expected = new() { { "one", 1 }, { "two", 2 }, { "three", 3 }, { "one", 1 } };
            MyMap<string, int> actual = firstMap + secondMap;
            Assert.AreEqual(expected, actual);
        }
    }
}