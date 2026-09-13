using UnityEngine;

namespace Zazerkalye.Visual
{
    /// <summary>Low-poly 3D figures from primitives — not photo billboards or voxelized portraits.</summary>
    public static class CharacterView
    {
        public static GameObject Attach(string id, Transform parent, float height)
        {
            var go = new GameObject(id);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;
            var kit = new Kit(go.transform);
            switch (id)
            {
                case "sator": BuildSator(kit); break;
                case "bobyl": BuildBobyl(kit, false); break;
                case "bobyl_hard": BuildBobyl(kit, true); break;
                case "jvachnik": BuildJvachnik(kit); break;
                case "polenych": BuildPolenych(kit, false); break;
                case "kolenych": BuildPolenych(kit, true); break;
                case "akaky": BuildAkaky(kit); break;
                case "kazimir": BuildKazimir(kit); break;
                case "mihail": BuildMihail(kit); break;
                case "pedal": BuildPedal(kit); break;
                case "scripach": BuildScripach(kit); break;
                case "istukanus": BuildIstukanus(kit); break;
                case "kukichi": BuildKukichi(kit); break;
                case "pacanoid": BuildPacanoid(kit); break;
                case "power": BuildPower(kit); break;
                default: BuildAkaky(kit); break;
            }
            go.transform.localScale = Vector3.one * height;
            AddShadow(go.transform);
            if (id != "kukichi" && id != "pacanoid" && id != "power" && id != "sator"
                && id != "bobyl" && id != "bobyl_hard" && id != "jvachnik")
                go.AddComponent<FigurineIdle>();
            return go;
        }

        static void AddShadow(Transform root)
        {
            var sh = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(sh.GetComponent<Collider>());
            sh.name = "BlobShadow";
            sh.transform.SetParent(root, false);
            sh.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            sh.transform.localScale = new Vector3(0.55f, 0.012f, 0.55f);
            sh.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Transparent(new Color(0f, 0f, 0f, 0.4f));
            sh.GetComponent<Renderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        public static void Label(Transform parent, string text, float y, Color color)
        {
            var go = new GameObject("Name");
            go.transform.SetParent(parent, false);
            go.transform.localPosition = new Vector3(0f, y, 0f);
            var tm = go.AddComponent<TextMesh>();
            tm.text = text;
            tm.fontSize = 42;
            tm.characterSize = 0.055f;
            tm.anchor = TextAnchor.LowerCenter;
            tm.alignment = TextAlignment.Center;
            tm.color = color;
            tm.fontStyle = FontStyle.Bold;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                tm.font = font;
                var rend = go.GetComponent<Renderer>();
                if (rend != null && font.material != null)
                    rend.sharedMaterial = font.material;
            }
            go.AddComponent<Billboard>();
        }

        sealed class Kit
        {
            public readonly Transform Root;
            public Kit(Transform root) => Root = root;

            public Transform Add(PrimitiveType type, string name, Vector3 pos, Vector3 scale, Color color, Vector3 euler = default)
            {
                var go = GameObject.CreatePrimitive(type);
                Object.Destroy(go.GetComponent<Collider>());
                go.name = name;
                go.transform.SetParent(Root, false);
                go.transform.localPosition = pos;
                go.transform.localRotation = Quaternion.Euler(euler);
                go.transform.localScale = scale;
                go.GetComponent<Renderer>().sharedMaterial = RuntimeMaterials.Painted(color);
                return go.transform;
            }

            public Transform Capsule(string n, Vector3 p, Vector3 s, Color c, Vector3 e = default) => Add(PrimitiveType.Capsule, n, p, s, c, e);
            public Transform Sphere(string n, Vector3 p, Vector3 s, Color c, Vector3 e = default) => Add(PrimitiveType.Sphere, n, p, s, c, e);
            public Transform Cylinder(string n, Vector3 p, Vector3 s, Color c, Vector3 e = default) => Add(PrimitiveType.Cylinder, n, p, s, c, e);
            public Transform Cube(string n, Vector3 p, Vector3 s, Color c, Vector3 e = default) => Add(PrimitiveType.Cube, n, p, s, c, e);
        }

        static readonly Color Coat = new(0.16f, 0.32f, 0.22f);
        static readonly Color CoatDark = new(0.10f, 0.20f, 0.14f);
        static readonly Color Crimson = new(0.58f, 0.14f, 0.12f);
        static readonly Color Belt = new(0.86f, 0.82f, 0.74f);
        static readonly Color Gold = new(0.78f, 0.62f, 0.28f);
        static readonly Color Skin = new(0.76f, 0.58f, 0.48f);
        static readonly Color Hair = new(0.18f, 0.12f, 0.10f);
        static readonly Color Shako = new(0.07f, 0.08f, 0.09f);
        static readonly Color Pants = new(0.12f, 0.13f, 0.14f);
        static readonly Color Boot = new(0.09f, 0.08f, 0.07f);
        static readonly Color Comb = new(0.48f, 0.28f, 0.14f);
        static readonly Color Shell = new(0.82f, 0.76f, 0.62f);
        static readonly Color ShellDark = new(0.55f, 0.48f, 0.36f);
        static readonly Color HardShell = new(0.16f, 0.12f, 0.12f);
        static readonly Color FaceGum = new(0.62f, 0.52f, 0.42f);
        static readonly Color SwampFur = new(0.28f, 0.38f, 0.24f);
        static readonly Color SwampDark = new(0.16f, 0.22f, 0.14f);
        static readonly Color Bark = new(0.32f, 0.22f, 0.12f);
        static readonly Color Moss = new(0.28f, 0.42f, 0.18f);
        static readonly Color Bone = new(0.86f, 0.82f, 0.72f);
        static readonly Color BoneDark = new(0.62f, 0.56f, 0.46f);
        static readonly Color Stone = new(0.38f, 0.38f, 0.36f);
        static readonly Color StoneDark = new(0.26f, 0.26f, 0.24f);
        static readonly Color Honey = new(0.86f, 0.58f, 0.18f);
        static readonly Color HoneyDark = new(0.55f, 0.32f, 0.08f);
        static readonly Color Lime = new(0.38f, 0.78f, 0.42f);
        static readonly Color LimeDark = new(0.22f, 0.48f, 0.26f);

        static void BuildSator(Kit k)
        {
            k.Cube("BootL", new Vector3(-0.09f, 0.05f, 0.04f), new Vector3(0.13f, 0.10f, 0.24f), Boot);
            k.Cube("BootR", new Vector3(0.09f, 0.05f, 0.04f), new Vector3(0.13f, 0.10f, 0.24f), Boot);
            k.Capsule("LegL", new Vector3(-0.09f, 0.22f, 0f), new Vector3(0.12f, 0.14f, 0.12f), Pants);
            k.Capsule("LegR", new Vector3(0.09f, 0.22f, 0f), new Vector3(0.12f, 0.14f, 0.12f), Pants);
            k.Capsule("Torso", new Vector3(0f, 0.52f, 0f), new Vector3(0.42f, 0.20f, 0.28f), Coat);
            k.Cube("SkirtL", new Vector3(-0.16f, 0.34f, -0.02f), new Vector3(0.14f, 0.22f, 0.20f), CoatDark);
            k.Cube("SkirtR", new Vector3(0.16f, 0.34f, -0.02f), new Vector3(0.14f, 0.22f, 0.20f), CoatDark);
            k.Cube("Collar", new Vector3(0f, 0.68f, 0.10f), new Vector3(0.22f, 0.10f, 0.08f), Crimson);
            k.Cube("CuffL", new Vector3(-0.38f, 0.42f, 0.02f), new Vector3(0.10f, 0.08f, 0.10f), Crimson);
            k.Cube("CuffR", new Vector3(0.38f, 0.42f, 0.02f), new Vector3(0.10f, 0.08f, 0.10f), Crimson);
            k.Cube("Belt", new Vector3(0f, 0.42f, 0.02f), new Vector3(0.40f, 0.05f, 0.30f), Belt);
            k.Cube("SashA", new Vector3(0f, 0.56f, 0.12f), new Vector3(0.08f, 0.36f, 0.04f), Belt, new Vector3(0f, 0f, 28f));
            k.Cube("SashB", new Vector3(0f, 0.56f, 0.13f), new Vector3(0.08f, 0.36f, 0.04f), Belt, new Vector3(0f, 0f, -28f));
            k.Sphere("EpauL", new Vector3(-0.22f, 0.70f, 0f), Vector3.one * 0.12f, Gold);
            k.Sphere("EpauR", new Vector3(0.22f, 0.70f, 0f), Vector3.one * 0.12f, Gold);
            k.Capsule("ArmL", new Vector3(-0.30f, 0.52f, 0f), new Vector3(0.11f, 0.14f, 0.11f), Coat, new Vector3(0f, 0f, 18f));
            k.Capsule("ArmR", new Vector3(0.30f, 0.52f, 0.04f), new Vector3(0.11f, 0.14f, 0.11f), Coat, new Vector3(12f, 0f, -22f));
            k.Sphere("HandL", new Vector3(-0.40f, 0.38f, 0.02f), Vector3.one * 0.09f, Skin);
            k.Sphere("HandR", new Vector3(0.40f, 0.36f, 0.10f), Vector3.one * 0.09f, Skin);
            k.Cube("Comb", new Vector3(0.48f, 0.30f, 0.12f), new Vector3(0.04f, 0.22f, 0.08f), Comb, new Vector3(20f, 0f, -15f));
            for (int i = 0; i < 5; i++)
                k.Cube("Tooth" + i, new Vector3(0.48f, 0.20f + i * 0.03f, 0.18f), new Vector3(0.02f, 0.015f, 0.10f), Comb, new Vector3(20f, 0f, -15f));
            k.Sphere("Head", new Vector3(0f, 0.80f, 0.02f), Vector3.one * 0.22f, Skin);
            k.Cube("StacheL", new Vector3(-0.04f, 0.76f, 0.11f), new Vector3(0.07f, 0.025f, 0.04f), Hair);
            k.Cube("StacheR", new Vector3(0.04f, 0.76f, 0.11f), new Vector3(0.07f, 0.025f, 0.04f), Hair);
            k.Sphere("Nose", new Vector3(0f, 0.79f, 0.12f), Vector3.one * 0.05f, Skin * 0.92f);
            k.Cylinder("Hat", new Vector3(0f, 0.96f, 0f), new Vector3(0.18f, 0.10f, 0.18f), Shako);
            k.Cube("Eagle", new Vector3(0f, 0.97f, 0.10f), new Vector3(0.10f, 0.08f, 0.02f), Gold);
            k.Sphere("Plume", new Vector3(0f, 1.10f, -0.02f), new Vector3(0.05f, 0.16f, 0.05f), Moss);
        }

        static void BuildBobyl(Kit k, bool hard)
        {
            var shell = hard ? HardShell : Shell;
            var shell2 = hard ? new Color(0.08f, 0.07f, 0.08f) : ShellDark;
            k.Sphere("FootL", new Vector3(-0.12f, 0.08f, 0.04f), new Vector3(0.16f, 0.12f, 0.18f), shell2);
            k.Sphere("FootR", new Vector3(0.12f, 0.08f, 0.04f), new Vector3(0.16f, 0.12f, 0.18f), shell2);
            k.Sphere("Egg", new Vector3(0f, 0.48f, 0f), new Vector3(0.62f, 0.78f, 0.58f), shell);
            k.Cube("CrackA", new Vector3(0.02f, 0.55f, 0.28f), new Vector3(0.02f, 0.42f, 0.04f), shell2, new Vector3(0f, 0f, 18f));
            k.Cube("CrackB", new Vector3(-0.08f, 0.62f, 0.26f), new Vector3(0.02f, 0.28f, 0.04f), shell2, new Vector3(0f, 0f, -32f));
            k.Sphere("Hole", new Vector3(0f, 0.42f, 0.22f), new Vector3(0.32f, 0.28f, 0.16f), FaceGum);
            k.Sphere("EyeL", new Vector3(-0.07f, 0.46f, 0.30f), Vector3.one * 0.07f, Color.black);
            k.Sphere("EyeR", new Vector3(0.07f, 0.46f, 0.30f), Vector3.one * 0.07f, Color.black);
            k.Cube("Mouth", new Vector3(0f, 0.36f, 0.30f), new Vector3(0.12f, 0.04f, 0.04f), new Color(0.35f, 0.12f, 0.12f));
            k.Sphere("Hair", new Vector3(0f, 0.52f, 0.18f), new Vector3(0.22f, 0.10f, 0.12f), new Color(0.45f, 0.40f, 0.32f));
        }

        static void BuildJvachnik(Kit k)
        {
            k.Capsule("Body", new Vector3(0f, 0.42f, -0.04f), new Vector3(0.38f, 0.28f, 0.55f), SwampFur, new Vector3(18f, 0f, 0f));
            k.Capsule("Neck", new Vector3(0f, 0.62f, 0.18f), new Vector3(0.18f, 0.12f, 0.18f), SwampFur, new Vector3(40f, 0f, 0f));
            k.Sphere("Head", new Vector3(0f, 0.72f, 0.38f), new Vector3(0.28f, 0.22f, 0.40f), SwampDark);
            k.Sphere("Jaw", new Vector3(0f, 0.62f, 0.48f), new Vector3(0.22f, 0.12f, 0.28f), SwampFur);
            k.Cube("EarL", new Vector3(-0.12f, 0.86f, 0.28f), new Vector3(0.06f, 0.16f, 0.10f), SwampDark, new Vector3(0f, 0f, -12f));
            k.Cube("EarR", new Vector3(0.12f, 0.86f, 0.28f), new Vector3(0.06f, 0.16f, 0.10f), SwampDark, new Vector3(0f, 0f, 12f));
            k.Sphere("EyeL", new Vector3(-0.08f, 0.76f, 0.52f), Vector3.one * 0.06f, new Color(0.85f, 0.20f, 0.12f));
            k.Sphere("EyeR", new Vector3(0.08f, 0.76f, 0.52f), Vector3.one * 0.06f, new Color(0.85f, 0.20f, 0.12f));
            k.Capsule("LegFL", new Vector3(-0.14f, 0.18f, 0.16f), new Vector3(0.10f, 0.16f, 0.10f), SwampDark);
            k.Capsule("LegFR", new Vector3(0.14f, 0.18f, 0.16f), new Vector3(0.10f, 0.16f, 0.10f), SwampDark);
            k.Capsule("LegBL", new Vector3(-0.14f, 0.18f, -0.22f), new Vector3(0.10f, 0.16f, 0.10f), SwampDark);
            k.Capsule("LegBR", new Vector3(0.14f, 0.18f, -0.22f), new Vector3(0.10f, 0.16f, 0.10f), SwampDark);
            k.Cube("Tail", new Vector3(0f, 0.38f, -0.42f), new Vector3(0.08f, 0.08f, 0.28f), SwampFur, new Vector3(20f, 0f, 0f));
        }

        static void BuildPolenych(Kit k, bool bone)
        {
            var wood = bone ? Bone : Bark;
            var wood2 = bone ? BoneDark : new Color(0.20f, 0.14f, 0.08f);
            k.Capsule("ShinL", new Vector3(-0.12f, 0.16f, 0.10f), new Vector3(0.14f, 0.14f, 0.14f), wood, new Vector3(18f, 0f, 0f));
            k.Capsule("ThighR", new Vector3(0.16f, 0.22f, -0.02f), new Vector3(0.16f, 0.16f, 0.16f), wood, new Vector3(70f, 20f, 0f));
            k.Sphere("Knee", new Vector3(0.28f, 0.12f, 0.16f), Vector3.one * 0.14f, wood2);
            k.Capsule("Torso", new Vector3(0f, 0.48f, 0f), new Vector3(0.36f, 0.22f, 0.28f), wood);
            k.Capsule("ArmL", new Vector3(-0.28f, 0.48f, 0.08f), new Vector3(0.12f, 0.16f, 0.12f), wood, new Vector3(0f, 0f, 35f));
            k.Capsule("ArmR", new Vector3(0.32f, 0.52f, 0.16f), new Vector3(0.12f, 0.16f, 0.12f), wood, new Vector3(20f, 0f, -50f));
            k.Sphere("HandR", new Vector3(0.46f, 0.38f, 0.28f), Vector3.one * 0.12f, wood);
            k.Sphere("Head", new Vector3(0f, 0.78f, 0.04f), Vector3.one * 0.28f, wood);
            k.Sphere("EyeL", new Vector3(-0.07f, 0.80f, 0.14f), Vector3.one * 0.07f, Color.black);
            k.Sphere("EyeR", new Vector3(0.07f, 0.80f, 0.14f), Vector3.one * 0.07f, Color.black);
            if (bone)
            {
                k.Cube("Rib", new Vector3(0f, 0.50f, 0.12f), new Vector3(0.28f, 0.04f, 0.04f), Bone);
                k.Cube("Rib2", new Vector3(0f, 0.44f, 0.12f), new Vector3(0.24f, 0.04f, 0.04f), Bone);
            }
            else
            {
                k.Sphere("Canopy", new Vector3(0.04f, 0.98f, 0f), new Vector3(0.42f, 0.28f, 0.38f), Moss);
                k.Sphere("Canopy2", new Vector3(-0.12f, 0.92f, -0.08f), Vector3.one * 0.28f, Moss * 1.1f);
                k.Sphere("MossArm", new Vector3(-0.22f, 0.58f, 0.06f), Vector3.one * 0.14f, Moss);
            }
        }

        static void BuildAkaky(Kit k)
        {
            var coat = new Color(0.38f, 0.32f, 0.22f);
            k.Capsule("LegL", new Vector3(-0.08f, 0.20f, 0f), new Vector3(0.12f, 0.16f, 0.12f), coat * 0.8f);
            k.Capsule("LegR", new Vector3(0.08f, 0.20f, 0f), new Vector3(0.12f, 0.16f, 0.12f), coat * 0.8f);
            k.Capsule("Torso", new Vector3(0f, 0.50f, 0f), new Vector3(0.36f, 0.20f, 0.28f), coat);
            k.Capsule("ArmL", new Vector3(-0.24f, 0.48f, 0f), new Vector3(0.10f, 0.14f, 0.10f), coat, new Vector3(0f, 0f, 12f));
            k.Capsule("ArmR", new Vector3(0.24f, 0.48f, 0f), new Vector3(0.10f, 0.14f, 0.10f), coat, new Vector3(0f, 0f, -12f));
            k.Sphere("Head", new Vector3(0f, 0.76f, 0.02f), Vector3.one * 0.22f, Skin);
            k.Sphere("Nose", new Vector3(0f, 0.74f, 0.12f), Vector3.one * 0.05f, Skin);
            k.Cube("Hair", new Vector3(0f, 0.86f, 0f), new Vector3(0.20f, 0.08f, 0.18f), new Color(0.42f, 0.32f, 0.18f));
        }

        static void BuildKazimir(Kit k)
        {
            var fur = new Color(0.28f, 0.22f, 0.30f);
            var steel = new Color(0.45f, 0.48f, 0.52f);
            k.Capsule("Body", new Vector3(0f, 0.40f, 0f), new Vector3(0.40f, 0.24f, 0.50f), fur, new Vector3(8f, 0f, 0f));
            k.Sphere("Head", new Vector3(0f, 0.62f, 0.28f), new Vector3(0.26f, 0.22f, 0.34f), fur);
            k.Sphere("Snout", new Vector3(0f, 0.56f, 0.46f), new Vector3(0.14f, 0.10f, 0.18f), fur * 1.1f);
            k.Cube("EarL", new Vector3(-0.10f, 0.76f, 0.22f), new Vector3(0.06f, 0.14f, 0.08f), fur);
            k.Cube("EarR", new Vector3(0.10f, 0.76f, 0.22f), new Vector3(0.06f, 0.14f, 0.08f), fur);
            k.Cube("PauldronL", new Vector3(-0.22f, 0.52f, 0.04f), new Vector3(0.16f, 0.10f, 0.22f), steel);
            k.Cube("PauldronR", new Vector3(0.22f, 0.52f, 0.04f), new Vector3(0.16f, 0.10f, 0.22f), steel);
            k.Capsule("LegL", new Vector3(-0.12f, 0.16f, 0.08f), new Vector3(0.12f, 0.14f, 0.12f), fur);
            k.Capsule("LegR", new Vector3(0.12f, 0.16f, 0.08f), new Vector3(0.12f, 0.14f, 0.12f), fur);
            k.Sphere("EyeL", new Vector3(-0.06f, 0.66f, 0.42f), Vector3.one * 0.05f, new Color(0.9f, 0.7f, 0.2f));
            k.Sphere("EyeR", new Vector3(0.06f, 0.66f, 0.42f), Vector3.one * 0.05f, new Color(0.9f, 0.7f, 0.2f));
            k.Cube("Spoon", new Vector3(0.28f, 0.28f, 0.16f), new Vector3(0.04f, 0.18f, 0.04f), steel, new Vector3(20f, 0f, 15f));
        }

        static void BuildMihail(Kit k)
        {
            var cloth = new Color(0.50f, 0.42f, 0.24f);
            k.Capsule("LegL", new Vector3(-0.08f, 0.20f, 0f), new Vector3(0.12f, 0.16f, 0.12f), cloth * 0.7f);
            k.Capsule("LegR", new Vector3(0.08f, 0.20f, 0f), new Vector3(0.12f, 0.16f, 0.12f), cloth * 0.7f);
            k.Capsule("Torso", new Vector3(0f, 0.50f, 0f), new Vector3(0.34f, 0.20f, 0.26f), cloth);
            k.Sphere("Head", new Vector3(0f, 0.76f, 0.02f), Vector3.one * 0.22f, new Color(0.80f, 0.68f, 0.52f));
            k.Sphere("Shell", new Vector3(0f, 0.52f, -0.22f), new Vector3(0.36f, 0.32f, 0.22f), Shell);
            k.Cube("Crack", new Vector3(0.02f, 0.55f, -0.32f), new Vector3(0.02f, 0.22f, 0.04f), ShellDark);
        }

        static void BuildPedal(Kit k)
        {
            var cloth = new Color(0.48f, 0.26f, 0.18f);
            k.Capsule("LegL", new Vector3(-0.08f, 0.18f, 0f), new Vector3(0.10f, 0.14f, 0.10f), cloth * 0.8f);
            k.Capsule("LegR", new Vector3(0.08f, 0.18f, 0f), new Vector3(0.10f, 0.14f, 0.10f), cloth * 0.8f);
            k.Capsule("Torso", new Vector3(0f, 0.44f, 0f), new Vector3(0.28f, 0.16f, 0.22f), cloth);
            k.Sphere("Head", new Vector3(0f, 0.66f, 0.02f), Vector3.one * 0.20f, Skin);
            k.Cube("Hair", new Vector3(0f, 0.76f, 0f), new Vector3(0.18f, 0.08f, 0.16f), Hair);
            k.Cylinder("Pedal", new Vector3(0.22f, 0.08f, 0.12f), new Vector3(0.22f, 0.03f, 0.08f), Gold, new Vector3(0f, 20f, 0f));
            k.Cube("Stem", new Vector3(0.22f, 0.16f, 0.12f), new Vector3(0.04f, 0.14f, 0.04f), Comb);
        }

        static void BuildScripach(Kit k)
        {
            var wood = new Color(0.22f, 0.26f, 0.20f);
            k.Capsule("Torso", new Vector3(0f, 0.48f, 0f), new Vector3(0.32f, 0.28f, 0.24f), wood);
            k.Capsule("LegL", new Vector3(-0.10f, 0.18f, 0f), new Vector3(0.10f, 0.16f, 0.10f), wood);
            k.Capsule("LegR", new Vector3(0.10f, 0.18f, 0f), new Vector3(0.10f, 0.16f, 0.10f), wood);
            k.Cube("Mask", new Vector3(0f, 0.82f, 0.10f), new Vector3(0.28f, 0.32f, 0.08f), new Color(0.42f, 0.30f, 0.16f));
            k.Sphere("EyeL", new Vector3(-0.07f, 0.86f, 0.15f), Vector3.one * 0.05f, new Color(0.9f, 0.15f, 0.1f));
            k.Sphere("EyeR", new Vector3(0.07f, 0.86f, 0.15f), Vector3.one * 0.05f, new Color(0.9f, 0.15f, 0.1f));
            k.Cube("Violin", new Vector3(0.22f, 0.50f, 0.12f), new Vector3(0.08f, 0.28f, 0.04f), Comb, new Vector3(20f, 0f, -25f));
        }

        static void BuildIstukanus(Kit k)
        {
            k.Cube("Plinth", new Vector3(0f, 0.12f, 0f), new Vector3(0.70f, 0.24f, 0.70f), StoneDark);
            k.Cube("Body", new Vector3(0f, 0.48f, 0f), new Vector3(0.48f, 0.50f, 0.32f), Stone);
            k.Cube("Head", new Vector3(0f, 0.86f, 0f), new Vector3(0.36f, 0.28f, 0.30f), Stone);
            k.Cube("ArmL", new Vector3(-0.32f, 0.46f, 0f), new Vector3(0.14f, 0.42f, 0.14f), Stone);
            k.Cube("ArmR", new Vector3(0.32f, 0.46f, 0f), new Vector3(0.14f, 0.42f, 0.14f), Stone);
            k.Cube("Plaque", new Vector3(0f, 0.12f, 0.36f), new Vector3(0.28f, 0.10f, 0.04f), new Color(0.22f, 0.24f, 0.22f));
            k.Sphere("Moss1", new Vector3(-0.10f, 0.96f, 0.08f), Vector3.one * 0.14f, Moss);
            k.Sphere("Moss2", new Vector3(0.18f, 0.40f, 0.16f), Vector3.one * 0.10f, Moss);
            k.Cube("Pendant", new Vector3(0f, 0.70f, 0.18f), new Vector3(0.10f, 0.12f, 0.04f), StoneDark);
        }

        static void BuildKukichi(Kit k)
        {
            k.Cylinder("Jar", new Vector3(0f, 0.28f, 0f), new Vector3(0.28f, 0.22f, 0.28f), Honey);
            k.Cylinder("Rim", new Vector3(0f, 0.50f, 0f), new Vector3(0.22f, 0.04f, 0.22f), HoneyDark);
            k.Sphere("Bite", new Vector3(0.16f, 0.30f, 0.10f), Vector3.one * 0.16f, Honey * 0.75f);
            k.Cube("Stick", new Vector3(0f, 0.62f, 0f), new Vector3(0.05f, 0.22f, 0.05f), Comb);
            var glow = k.Sphere("Glow", new Vector3(0f, 0.28f, 0f), Vector3.one * 0.42f, new Color(1f, 0.75f, 0.25f, 0.25f));
            glow.GetComponent<Renderer>().sharedMaterial =
                RuntimeMaterials.Transparent(new Color(1f, 0.72f, 0.2f, 0.22f));
        }

        static void BuildPacanoid(Kit k)
        {
            k.Sphere("A", new Vector3(-0.22f, 0.28f, 0f), Vector3.one * 0.42f, Lime);
            k.Sphere("B", new Vector3(0.22f, 0.28f, 0f), Vector3.one * 0.42f, Lime * 1.08f);
            k.Sphere("EyeA", new Vector3(-0.22f, 0.34f, 0.18f), Vector3.one * 0.10f, LimeDark);
            k.Sphere("EyeB", new Vector3(0.22f, 0.34f, 0.18f), Vector3.one * 0.10f, LimeDark);
            k.Cube("Link", new Vector3(0f, 0.22f, 0f), new Vector3(0.18f, 0.06f, 0.06f), LimeDark);
        }

        static void BuildPower(Kit k)
        {
            k.Cube("Gem", new Vector3(0f, 0.28f, 0f), Vector3.one * 0.32f, new Color(0.55f, 0.78f, 0.95f), new Vector3(45f, 45f, 0f));
            k.Cube("Gem2", new Vector3(0f, 0.28f, 0f), Vector3.one * 0.22f, new Color(0.85f, 0.55f, 0.75f), new Vector3(45f, -20f, 15f));
        }
    }
}
