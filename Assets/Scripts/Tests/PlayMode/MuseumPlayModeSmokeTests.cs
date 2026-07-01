using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

/// <summary>
/// Lightweight Play Mode smoke tests (no scene file required).
/// </summary>
public class MuseumPlayModeSmokeTests
{
    [SetUp]
    public void SetUp()
    {
        MuseumTimeScale.ForceUnfreeze();
    }

    [TearDown]
    public void TearDown()
    {
        MuseumTimeScale.ForceUnfreeze();
    }

    [Test]
    public void TimeScale_PushPopFreezesAndRestores()
    {
        MuseumTimeScale.PushFreeze();
        Assert.AreEqual(0f, Time.timeScale);
        MuseumTimeScale.PopFreeze();
        Assert.AreEqual(1f, Time.timeScale);
    }

    [UnityTest]
    public IEnumerator PlacementPopFeedback_ReturnsToBaseScale()
    {
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        Vector3 baseScale = cube.transform.localScale;

        PlacementPopFeedback.Play(cube.transform);
        yield return new WaitForSeconds(0.35f);

        Assert.AreEqual(baseScale, cube.transform.localScale);
        Object.Destroy(cube);
    }

    [UnityTest]
    public IEnumerator SortingTable_StagesPaintingOnGrid()
    {
        var tableObject = new GameObject("TestSortingTable");
        var box = tableObject.AddComponent<BoxCollider>();
        box.size = new Vector3(4f, 0.1f, 4f);
        box.isTrigger = true;
        SortingTable table = tableObject.AddComponent<SortingTable>();

        var paintingObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        paintingObject.transform.position = tableObject.transform.position;
        paintingObject.AddComponent<InteractablePainting>();
        paintingObject.AddComponent<Rigidbody>();

        bool staged = table.TryStagePainting(paintingObject.transform);
        yield return null;

        Assert.IsTrue(staged);
        Assert.IsTrue(SortingTable.IsStaged(paintingObject.transform));
        Assert.AreEqual(tableObject.transform, paintingObject.transform.parent);

        Object.Destroy(paintingObject);
        Object.Destroy(tableObject);
    }
}
