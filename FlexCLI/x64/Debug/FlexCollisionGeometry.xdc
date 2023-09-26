<?xml version="1.0"?><doc>
<members>
<member name="M:FlexCLI.FlexScene.#ctor" decl="true" source="D:\Application Development\grasshopper development\TwinLand_win\FlexCLI\FlexCLI.h" line="187">
<summary>Empty constructor</summary>
</member>
<member name="M:FlexCLI.FlexScene.NumParticles" decl="false" source="D:\Application Development\grasshopper development\TwinLand_win\FlexCLI\FlexCLI.h" line="190">
<summary>Number of all particles in the scene</summary>
</member>
<member name="M:FlexCLI.FlexSolverOptions.#ctor(System.Single,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32[],System.Single)" decl="true" source="D:\Application Development\grasshopper development\TwinLand_win\FlexCLI\FlexCLI.h" line="307">
<summary>
memoryRequirements: [0] MaxParticles, [1] MaxNeighborperParticle, [2] MaxCollisionShapeNumber, [3] MaxCollisionMeshVertexCount, [4] MaxCollisionMeshIndexCount, [5] MaxCollisionConvexShapePlanes, [6] MaxRigidBodies, [7] MaxSprings, [8] MaxDynamicTriangles
</summary>
</member>
<member name="M:FlexCLI.FlexCollisionGeometry.AddPlane(System.Single,System.Single,System.Single,System.Single)" decl="false" source="D:\Application Development\grasshopper development\TwinLand_win\FlexCLI\FlexCollisionGeometry.cpp" line="15">
<summary>
Add up to eight collision planes, each in the form: Ax + By + Cz + D = 0. Anything beyond eight planes will be ignored.
</summary>
</member>
<member name="M:FlexCLI.FlexCollisionGeometry.AddSphere(System.Single[],System.Single)" decl="false" source="D:\Application Development\grasshopper development\TwinLand_win\FlexCLI\FlexCollisionGeometry.cpp" line="29">
<summary>
Add a sphere by its center position and radius.
</summary>
</member>
<member name="M:FlexCLI.FlexCollisionGeometry.AddBox(System.Single[],System.Single[],System.Single[])" decl="false" source="D:\Application Development\grasshopper development\TwinLand_win\FlexCLI\FlexCollisionGeometry.cpp" line="45">
<summary>
Add a box by its extends in each dimension, center position and orientation.
</summary>
</member>
<member name="M:FlexCLI.FlexCollisionGeometry.AddCapsule(System.Single,System.Single,System.Single[],System.Single[])" decl="false" source="D:\Application Development\grasshopper development\TwinLand_win\FlexCLI\FlexCollisionGeometry.cpp" line="68">
<summary>
UNTESTED: Add a capsule by its extends in X, radius, center position and orientation.
</summary>
</member>
<member name="M:FlexCLI.FlexCollisionGeometry.AddMesh(System.Single[],System.Int32[])" decl="false" source="D:\Application Development\grasshopper development\TwinLand_win\FlexCLI\FlexCollisionGeometry.cpp" line="91">
<summary>
Add a triangle mesh by its vertex position and faces both as flattened arrays. Make sure front face CCW is pointing outward otherwise results are unforeseen.
</summary>
</member>
<member name="M:FlexCLI.FlexCollisionGeometry.AddConvexShape(System.Single[],System.Single[],System.Single[])" decl="false" source="D:\Application Development\grasshopper development\TwinLand_win\FlexCLI\FlexCollisionGeometry.cpp" line="131">
<summary>
Add a convex mesh by the plane of each mesh face in the form ABCD (z+ should point inward) in a flattened array. upper and lower limits (float[3]) refer to vertex positions
</summary>
</member>
</members>
</doc>