<?xml version="1.0"?><doc>
<members>
<member name="M:FlexCLI.FlexScene.#ctor" decl="true" source="D:\Application Development\grasshopper development\TwinLand\FlexCLI\FlexCLI.h" line="187">
<summary>Empty constructor</summary>
</member>
<member name="M:FlexCLI.FlexScene.NumParticles" decl="false" source="D:\Application Development\grasshopper development\TwinLand\FlexCLI\FlexCLI.h" line="190">
<summary>Number of all particles in the scene</summary>
</member>
<member name="M:FlexCLI.FlexSolverOptions.#ctor(System.Single,System.Int32,System.Int32,System.Int32,System.Int32,System.Int32[],System.Single)" decl="true" source="D:\Application Development\grasshopper development\TwinLand\FlexCLI\FlexCLI.h" line="307">
<summary>
memoryRequirements: [0] MaxParticles, [1] MaxNeighborperParticle, [2] MaxCollisionShapeNumber, [3] MaxCollisionMeshVertexCount, [4] MaxCollisionMeshIndexCount, [5] MaxCollisionConvexShapePlanes, [6] MaxRigidBodies, [7] MaxSprings, [8] MaxDynamicTriangles
</summary>
</member>
<member name="M:FlexCLI.SimBuffers.Allocate" decl="false" source="D:\Application Development\grasshopper development\TwinLand\FlexCLI\FlexCLI.cpp" line="67">
Tells the host upon startup, how much memory it will need and reserves this memory
</member>
<member name="M:FlexCLI.SimBuffers.Destroy" decl="false" source="D:\Application Development\grasshopper development\TwinLand\FlexCLI\FlexCLI.cpp" line="101">
<summary>
Performs the following steps for every buffer: Check if pointer is 0; if it is, do nothing. If it is not, free buffer (NvFlex function) and set pointer to 0.
</summary>
</member>
<member name="M:FlexCLI.Flex.#ctor" decl="false" source="D:\Application Development\grasshopper development\TwinLand\FlexCLI\FlexCLI.cpp" line="230">
<summary>Create a default Flex engine object. This will initialize a solver, create buffers and set up default NvFlexParams.</summary>
</member>
<member name="M:FlexCLI.Flex.IsReady" decl="false" source="D:\Application Development\grasshopper development\TwinLand\FlexCLI\FlexCLI.cpp" line="312">
<summary>Returns true if pointers to library and solver objects are valid</summary>
</member>
<member name="M:FlexCLI.Flex.SetCollisionGeometry(FlexCLI.FlexCollisionGeometry)" decl="false" source="D:\Application Development\grasshopper development\TwinLand\FlexCLI\FlexCLI.cpp" line="317">
Registration methods private and public
<summary>Register different collision geometries wrapped into the FlexCollisionGeometry class.</summary>
</member>
<member name="M:FlexCLI.Flex.SetParams(FlexCLI.FlexParams)" decl="false" source="D:\Application Development\grasshopper development\TwinLand\FlexCLI\FlexCLI.cpp" line="510">
<summary>Register simulation parameters using the FlexCLI.FlexParams class</summary>
</member>
<member name="M:FlexCLI.Flex.SetScene(FlexCLI.FlexScene)" decl="false" source="D:\Application Development\grasshopper development\TwinLand\FlexCLI\FlexCLI.cpp" line="570">
<summary>Register a simulation scenery using the FlexCLI.FlexScene class</summary>
</member>
</members>
</doc>