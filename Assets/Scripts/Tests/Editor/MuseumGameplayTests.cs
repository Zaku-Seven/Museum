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
            Object.DestroyImmediate(mountA.Root);
            Object.DestroyImmediate(mountB.Root);
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
            Object.DestroyImmediate(filled.Root);
            Object.DestroyImmediate(emptyMountObject);
        }
    }

    [Test]
    public void EntityRegistry_FindsRegisteredId()
    {
        var entityObject = new GameObject("EntityTest");
        try
        {
            MuseumEntityId entity = entityObject.AddComponent<MuseumEntityId>();
            entity.SetEntityId("test_entity_01");
            entity.enabled = true;

            MuseumEntityId found = MuseumEntityId.Registry.Find("test_entity_01");
            Assert.IsNotNull(found);
            Assert.AreEqual(entity, found);
        }
        finally
        {
            Object.DestroyImmediate(entityObject);
            MuseumEntityId.Registry.ClearForTests();
        }
    }

    [Test]
    public void CanAccept_SpecificSlot_WrongPainting_ReturnsFalse()
    {
        var mountObject = new GameObject("TestMount");
        var paintingObject = new GameObject("TestPainting");
        try
        {
            PaintingMount mount = mountObject.AddComponent<PaintingMount>();
            InteractablePainting painting = paintingObject.AddComponent<InteractablePainting>();
            paintingObject.AddComponent<Rigidbody>();
            paintingObject.AddComponent<MuseumEntityId>().SetEntityId("painting_cobalt_field");

            SetPrivateEnum(mount, "requiredWing", GalleryWing.Modern);
            SetPrivateEnum(painting, "wing", GalleryWing.Modern);
            SetPrivateString(mount, "requiredPaintingId", "painting_steel_lines");

            Assert.IsFalse(mount.CanAccept(painting));
        }
        finally
        {
            Object.DestroyImmediate(mountObject);
            Object.DestroyImmediate(paintingObject);
            MuseumEntityId.Registry.ClearForTests();
        }
    }

    [Test]
    public void CanAccept_SpecificSlot_MatchingPainting_ReturnsTrue()
    {
        var mountObject = new GameObject("TestMount");
        var paintingObject = new GameObject("TestPainting");
        try
        {
            PaintingMount mount = mountObject.AddComponent<PaintingMount>();
            InteractablePainting painting = paintingObject.AddComponent<InteractablePainting>();
            paintingObject.AddComponent<Rigidbody>();
            paintingObject.AddComponent<MuseumEntityId>().SetEntityId("painting_cobalt_field");

            SetPrivateEnum(mount, "requiredWing", GalleryWing.Modern);
            SetPrivateEnum(painting, "wing", GalleryWing.Modern);
            SetPrivateString(mount, "requiredPaintingId", "painting_cobalt_field");

            Assert.IsTrue(mount.CanAccept(painting));
        }
        finally
        {
            Object.DestroyImmediate(mountObject);
            Object.DestroyImmediate(paintingObject);
            MuseumEntityId.Registry.ClearForTests();
        }
    }

    [Test]
    public void HangProgress_CountsCorrectlyOccupiedMounts()
    {
        var mountA = new GameObject("MountA");
        var mountB = new GameObject("MountB");
        var paintingA = new GameObject("PaintingA");
        var paintingB = new GameObject("PaintingB");
        try
        {
            PaintingMount mountComponentA = mountA.AddComponent<PaintingMount>();
            PaintingMount mountComponentB = mountB.AddComponent<PaintingMount>();
            InteractablePainting paintingComponentA = paintingA.AddComponent<InteractablePainting>();
            InteractablePainting paintingComponentB = paintingB.AddComponent<InteractablePainting>();
            paintingA.AddComponent<Rigidbody>();
            paintingB.AddComponent<Rigidbody>();

            SetPrivateEnum(mountComponentA, "requiredWing", GalleryWing.Modern);
            SetPrivateEnum(mountComponentB, "requiredWing", GalleryWing.Modern);
            SetPrivateEnum(paintingComponentA, "wing", GalleryWing.Modern);
            SetPrivateEnum(paintingComponentB, "wing", GalleryWing.Classical);

            mountComponentA.PlacePainting(paintingA.transform, paintingA.GetComponent<Rigidbody>());
            mountComponentB.PlacePainting(paintingB.transform, paintingB.GetComponent<Rigidbody>());

            Assert.AreEqual(2, MuseumHangProgress.TotalMountSlots);
            Assert.AreEqual(1, MuseumHangProgress.CorrectlyHungCount);
            Assert.AreEqual(50, MuseumHangProgress.CompletionPercent);
        }
        finally
        {
            Object.DestroyImmediate(mountA);
            Object.DestroyImmediate(mountB);
            Object.DestroyImmediate(paintingA);
            Object.DestroyImmediate(paintingB);
        }
    }

    [Test]
    public void WingGuide_CountsAcceptingMounts()
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

            int count = MuseumWingGuide.CountAcceptingMounts(painting, new[] { mount });
            Assert.AreEqual(1, count);
            Assert.IsFalse(string.IsNullOrEmpty(MuseumWingGuide.BuildGuidance(painting, new[] { mount })));
        }
        finally
        {
            Object.DestroyImmediate(mountObject);
            Object.DestroyImmediate(paintingObject);
        }
    }

    [Test]
    public void PlayerSettingsStore_ResetToDefaults_RestoresDefaults()
    {
        PlayerSettingsStore.MouseSensitivity = 7f;
        PlayerSettingsStore.ResetToDefaults();
        Assert.AreEqual(PlayerSettingsStore.DefaultMouseSensitivity, PlayerSettingsStore.MouseSensitivity);
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

    private static void SetPrivateString(Object target, string fieldName, string value)
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

}
