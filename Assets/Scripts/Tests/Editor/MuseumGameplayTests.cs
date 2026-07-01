using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Edit-mode tests for mount acceptance and section completion math (no Play mode required).
/// </summary>
public class MuseumGameplayTests
{
    [Test]
    public void CanAccept_EmptyMatchingWing_ReturnsTrue()
    {
        var mountObject = new GameObject("TestMount");
        var paintingObject = new GameObject("TestPainting");
        try
        {
            PaintingMount mount = mountObject.AddComponent<PaintingMount>();
            InteractablePainting painting = paintingObject.AddComponent<InteractablePainting>();
            paintingObject.AddComponent<Rigidbody>();

            SetPrivateEnum(mount, "requiredWing", GalleryWing.Modern);
            SetPrivateEnum(painting, "wing", GalleryWing.Modern);

            Assert.IsTrue(mount.CanAccept(painting));
        }
        finally
        {
            Object.DestroyImmediate(mountObject);
            Object.DestroyImmediate(paintingObject);
        }
    }

    [Test]
    public void CanAccept_WrongWing_ReturnsFalse()
    {
        var mountObject = new GameObject("TestMount");
        var paintingObject = new GameObject("TestPainting");
        try
        {
            PaintingMount mount = mountObject.AddComponent<PaintingMount>();
            InteractablePainting painting = paintingObject.AddComponent<InteractablePainting>();
            paintingObject.AddComponent<Rigidbody>();

            SetPrivateEnum(mount, "requiredWing", GalleryWing.Modern);
            SetPrivateEnum(painting, "wing", GalleryWing.Classical);

            Assert.IsFalse(mount.CanAccept(painting));
        }
        finally
        {
            Object.DestroyImmediate(mountObject);
            Object.DestroyImmediate(paintingObject);
        }
    }

    [Test]
    public void CanAccept_NullPainting_ReturnsFalse()
    {
        var mountObject = new GameObject("TestMount");
        try
        {
            PaintingMount mount = mountObject.AddComponent<PaintingMount>();
            Assert.IsFalse(mount.CanAccept(null));
        }
        finally
        {
            Object.DestroyImmediate(mountObject);
        }
    }

    [Test]
    public void Section_CompletesWhenAllMountsFilledWithMatchingWing()
    {
        var sectionObject = new GameObject("TestSection");
        var mountA = CreateMountWithPainting(GalleryWing.Classical, GalleryWing.Classical);
        var mountB = CreateMountWithPainting(GalleryWing.Classical, GalleryWing.Classical);

        try
        {
            GallerySection section = sectionObject.AddComponent<GallerySection>();
            SetPrivateEnum(section, "sectionWing", GalleryWing.Classical);
            SetPrivateList(section, "mounts", new[] { mountA.Mount, mountB.Mount });

            mountA.Mount.SetSection(section);
            mountB.Mount.SetSection(section);
            section.CheckComplete();

            Assert.IsTrue(section.IsComplete);
            Assert.AreEqual(2, section.CorrectlyFilledCount());
        }
        finally
        {
            Object.DestroyImmediate(sectionObject);
            mountA.Root.DestroyImmediateSafe();
            mountB.Root.DestroyImmediateSafe();
        }
    }

    [Test]
    public void Section_IncompleteWhenOneMountEmpty()
    {
        var sectionObject = new GameObject("TestSection");
        var filled = CreateMountWithPainting(GalleryWing.Impressionist, GalleryWing.Impressionist);
        var emptyMountObject = new GameObject("EmptyMount");
        PaintingMount emptyMount = emptyMountObject.AddComponent<PaintingMount>();

        try
        {
            GallerySection section = sectionObject.AddComponent<GallerySection>();
            SetPrivateEnum(section, "sectionWing", GalleryWing.Impressionist);
            SetPrivateList(section, "mounts", new[] { filled.Mount, emptyMount });

            filled.Mount.SetSection(section);
            emptyMount.SetSection(section);
            section.CheckComplete();

            Assert.IsFalse(section.IsComplete);
            Assert.AreEqual(1, section.CorrectlyFilledCount());
        }
        finally
        {
            Object.DestroyImmediate(sectionObject);
            filled.Root.DestroyImmediateSafe();
            Object.DestroyImmediate(emptyMountObject);
        }
    }

    private static (PaintingMount Mount, GameObject Root) CreateMountWithPainting(GalleryWing mountWing, GalleryWing paintingWing)
    {
        var mountRoot = new GameObject("Mount");
        var paintingRoot = new GameObject("Painting");
        paintingRoot.transform.SetParent(mountRoot.transform);

        PaintingMount mount = mountRoot.AddComponent<PaintingMount>();
        InteractablePainting painting = paintingRoot.AddComponent<InteractablePainting>();
        paintingRoot.AddComponent<Rigidbody>();

        SetPrivateEnum(mount, "requiredWing", mountWing);
        SetPrivateEnum(painting, "wing", paintingWing);

        mount.PlacePainting(paintingRoot.transform, paintingRoot.GetComponent<Rigidbody>());
        return (mount, mountRoot);
    }

    private static void SetPrivateEnum(Object target, string fieldName, GalleryWing value)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field {fieldName} not found on {target.GetType().Name}");
        field.SetValue(target, value);
    }

    private static void SetPrivateList(Object target, string fieldName, PaintingMount[] mounts)
    {
        var field = target.GetType().GetField(fieldName,
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        Assert.IsNotNull(field, $"Field {fieldName} not found on {target.GetType().Name}");
        field.SetValue(target, new System.Collections.Generic.List<PaintingMount>(mounts));
    }

    private static void DestroyImmediateSafe(this GameObject gameObject)
    {
        if (gameObject != null)
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}
