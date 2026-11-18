"""
Harvest Hollow - low-poly cozy smoke/puff meshes for Unity mesh particles.
Run: python smoke_gen.py -- <out_dir>   (bpy module)  or  blender -b -P smoke_gen.py -- <out_dir>

Exports SmokePuffs.fbx with these meshes, each pivoted at its centre and roughly 1 m wide:
  Puff_01..Puff_03   lumpy faceted blobs with slightly flattened bottoms (80 tris)
  Puff_04            chunky 20-tri bit for small wisps / dust
  Puff_Cluster       three blobs fused, for big "cover" puffs
  Sparkle_Star       chunky 4-point star, for the "restored!" twinkle
"""
import bpy, bmesh, math, random, os, sys
from mathutils import Vector, Matrix

argv = sys.argv[sys.argv.index("--") + 1:] if "--" in sys.argv else []
OUT = os.path.abspath(argv[0]) if argv else os.getcwd()
os.makedirs(OUT, exist_ok=True)
rng = random.Random(4)

bpy.ops.wm.read_factory_settings(use_empty=True)
COL = bpy.context.scene.collection

mat = bpy.data.materials.new("FX_Puff")
mat.diffuse_color = (1, 1, 1, 1)

def blob(bm, center, radius, subdiv, lump, squash=0.88, floor=-0.62):
    geom = bmesh.ops.create_icosphere(bm, subdivisions=subdiv, radius=radius)
    verts = geom["verts"]
    for v in verts:
        n = v.co.normalized()
        v.co = n * radius * (1 + rng.uniform(-lump, lump))
        v.co.z *= squash
        v.co.z = max(v.co.z, floor * radius)          # cosy flat-ish underside
        v.co += Vector(center)
    return verts

def make(name, build, x):
    me = bpy.data.meshes.new(name)
    bm = bmesh.new()
    build(bm)
    bmesh.ops.remove_doubles(bm, verts=bm.verts, dist=1e-4)
    bmesh.ops.recalc_face_normals(bm, faces=bm.faces)
    bm.to_mesh(me); bm.free()
    for p in me.polygons:
        p.use_smooth = False
    me.materials.append(mat)
    ob = bpy.data.objects.new(name, me)
    COL.objects.link(ob)
    ob.location = (x, 0, 0)
    return ob

objs = []
objs.append(make("Puff_01", lambda bm: blob(bm, (0, 0, 0), 0.5, 2, 0.10), 0))
objs.append(make("Puff_02", lambda bm: blob(bm, (0, 0, 0), 0.5, 2, 0.16, squash=0.8), 1.4))
objs.append(make("Puff_03", lambda bm: blob(bm, (0, 0, 0), 0.5, 2, 0.07), 2.8))
objs.append(make("Puff_04", lambda bm: blob(bm, (0, 0, 0), 0.5, 1, 0.12, squash=0.85), 4.2))

def cluster(bm):
    blob(bm, (-0.22, 0.02, -0.05), 0.32, 2, 0.12)
    blob(bm, (0.2, -0.04, -0.07), 0.3, 2, 0.12)
    blob(bm, (0.0, 0.05, 0.12), 0.36, 2, 0.1)
objs.append(make("Puff_Cluster", cluster, 5.8))

def star(bm):
    outer, inner, t = 0.5, 0.16, 0.12
    ring = []
    for i in range(8):
        a = math.pi / 2 + i * math.pi / 4
        r = outer if i % 2 == 0 else inner
        ring.append((math.cos(a) * r, 0, math.sin(a) * r))
    front = bm.verts.new((0, -t / 2, 0)); back = bm.verts.new((0, t / 2, 0))
    rv = [bm.verts.new(p) for p in ring]
    for i in range(8):
        a, b = rv[i], rv[(i + 1) % 8]
        bm.faces.new((front, a, b)); bm.faces.new((back, b, a))
objs.append(make("Sparkle_Star", star, 7.2))

bpy.ops.object.select_all(action="DESELECT")
for o in objs:
    o.select_set(True)
fbx = os.path.join(OUT, "SmokePuffs.fbx")
bpy.ops.export_scene.fbx(filepath=fbx, use_selection=True, object_types={"MESH"},
                         apply_unit_scale=True, apply_scale_options="FBX_SCALE_ALL",
                         axis_forward="-Z", axis_up="Y", bake_space_transform=True,
                         mesh_smooth_type="FACE", add_leaf_bones=False, bake_anim=False)
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, "SmokePuffs.blend"))
for o in objs:
    print("TRIS", o.name, sum(len(p.vertices) - 2 for p in o.data.polygons))
print("EXPORTED", fbx)
