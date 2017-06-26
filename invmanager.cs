public void Write(IMyTextPanel panel, string text) {
	if (text == null)
	{
		return;
	}
	else
	{
		if (panel == null)
		{
			Echo(text);
		}
		else
		{
			panel.WritePublicText(text, true);
			panel.ShowPublicTextOnScreen();
		}
	}
}

public void WriteLine(IMyTextPanel panel, string text) {
	Write(panel, text + '\n');
}

public void Clear(IMyTextPanel panel) {
	panel.WritePublicText("");
	panel.ShowPublicTextOnScreen();
}

public bool CheckForLabel(string name, string id) {
	return String.Equals(name.Substring(name.Length - id.Length, id.Length), id, StringComparison.OrdinalIgnoreCase);
}

public void TransferAll(IMyInventory source, IMyInventory dst) {
	if (dst == null)
	{
		return;
	}
	var items = source.GetItems();
	for (int i = 0; i < items.Count; i++)
	{
		source.TransferItemTo(dst, i);
		WriteLine(debugScreen, "Transferred: " + items[i].Amount + ' ' + items[i].Content.SubtypeName);
	}
}

public class BlockInterface {
	private IMyTerminalBlock block;
	private string name;
	private IMyInventory input;
	private bool hasOutput = false;
	private IMyInventory output;
	private int priority = 1;
	public bool[] labels = new bool[(int)Labeling.NumOfLabels];
	
	public BlockInterface(IMyTerminalBlock ablock)
	{
		block = ablock;
		name = ablock.CustomName;
		input = ablock.GetInventory(0);
		if (ablock.InventoryCount > 1)
		{
			hasOutput = true;
			output = ablock.GetInventory(1);
		}
		for (int i = 0; i < (int)Labeling.NumOfLabels; i++)
		{
			labels[i] = false;
		}
	}
	
	public IMyTerminalBlock Terminal {
		get {return block; }
	}
	public string Type {
		get { return block.DefinitionDisplayNameText; }
	}
	public IMyInventory Input {
		get { return input; }
	}
	public bool HasOutput {
		get { return hasOutput; }
	}
	public IMyInventory Output {
		get { return output; }
	}
	public string Name {
		get { return name; }
		set { name = value; }
	}
	public int Priority {
		get { return priority; }
		set { priority = value; }
	}
}

public class Quota {
	private double now;
	private double needed;
	
	public Quota(double req) {
		now = 0;
		needed = req;
	}
	
	public double Now {
		get { return now; }
		set { now = value; }
	}
	public double Req {
		get { return needed; }
	}
	public bool Enough() {
		return now >= needed;
	}
}

public enum Labeling {
	Ore,
	Ingot,
	Component,
	NumOfLabels
}

public enum MaterialType {
	Stone,
	Iron,
	Nickel,
	Cobalt,
	Magnesium,
	Silicon,
	Silver,
	Gold,
	Platinum,
	Uranium,
	NumOfMaterialType
}

static int num;
static IMyTextPanel debugScreen;
static IMyTextPanel oreScreen;
static IMyTextPanel ingotScreen;
static IMyTextPanel compScreen;
static Dictionary<string, MyDefinitionId> compItems = new Dictionary<string, MyDefinitionId>();

public Program() {
	debugScreen = GridTerminalSystem.GetBlockWithName("LCD Debug") as IMyTextPanel;
	oreScreen = GridTerminalSystem.GetBlockWithName("LCD Ores") as IMyTextPanel;
	ingotScreen = GridTerminalSystem.GetBlockWithName("LCD Ingots") as IMyTextPanel;
	compScreen = GridTerminalSystem.GetBlockWithName("LCD Components") as IMyTextPanel;
	WriteLine(debugScreen, "---------- Ptrsn's Inventory Manager v0.0001 ----------");
	compItems.Add("Computer", MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + "ComputerComponent"));
	compItems.Add("Construction", MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + "ConstructionComponent"));
	compItems.Add("Detector", MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + "DetectorComponent"));
	compItems.Add("Explosives", MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + "ExplosivesComponent"));
	compItems.Add("Girder", MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + "GirderComponent"));
	compItems.Add("GravityGenerator", MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + "GravityGeneratorComponent"));
	compItems.Add("Medical", MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + "MedicalComponent"));
	compItems.Add("Motor", MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + "MotorComponent"));
	compItems.Add("RadioCommunication", MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + "RadioCommunicationComponent"));
	compItems.Add("Reactor", MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + "ReactorComponent"));
	compItems.Add("Thrust", MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + "ThrustComponent"));
}

public void Save() {
}

public void Main() {
	// TODO List:
	// Scrap
	// Relative change
	// Critical ingot amount -> Try refine that
	// Auto-assemble check for ingots
	
	DateTime startTime = DateTime.Now;
	// Init
	// Get all blocks with inventories
	var blocks = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocks(blocks);
	// TODO: Make this global
	var blkInts = new List<BlockInterface>();
	foreach (var block in blocks)
	{
		if (block.HasInventory)
		{
			blkInts.Add(new BlockInterface(block));
		}
	}
	
	// Consts
	int barSize = 15;
	string ambMainLabel = "!main";
	
	var refrates = new Dictionary<string, int>();
	// Per hour
	refrates.Add("Stone", 46800);
	refrates.Add("Iron", 93600);
	refrates.Add("Silicon", 7800);
	refrates.Add("Nickel", 2340);
	refrates.Add("Cobalt", 1170);
	refrates.Add("Silver", 4680);
	refrates.Add("Gold", 11700);
	refrates.Add("Magnesium", 4680);
	refrates.Add("Uranium", 1170);
	refrates.Add("Platinum", 1170);
	refrates.Add("Scrap", 117000);
	
	var components = new Dictionary<string, Quota>();
	components.Add("Computer", new Quota(1000));
	components.Add("Construction", new Quota(1000));
	components.Add("Detector", new Quota(10));
	components.Add("Display", new Quota(200));
	components.Add("Explosives", new Quota(10));
	components.Add("Girder", new Quota(100));
	components.Add("GravityGenerator", new Quota(10));
	components.Add("InteriorPlate", new Quota(1000));
	components.Add("LargeTube", new Quota(200));
	components.Add("Medical", new Quota(10));
	components.Add("MetalGrid", new Quota(200));
	components.Add("Motor", new Quota(500));
	components.Add("PowerCell", new Quota(10));
	components.Add("RadioCommunication", new Quota(10));
	components.Add("Reactor", new Quota(10));
	components.Add("SmallTube", new Quota(200));
	components.Add("SolarCell", new Quota(10));
	components.Add("SteelPlate", new Quota(1000));
	components.Add("Superconductor", new Quota(10));
	components.Add("Thrust", new Quota(10));
	
	// Variables
	double[,] counters = new double[2, (int)MaterialType.NumOfMaterialType];
	IMyInventory[] containers = new IMyInventory[(int)Labeling.NumOfLabels];
	
	// Refresh screens
	Clear(debugScreen);
	Clear(oreScreen);
	Clear(ingotScreen);
	Clear(compScreen);
	
	// Reset counter
	foreach (var comp in components)
	{
		comp.Value.Now = 0;
	}
	
	// Look for labels (!)
	for (int i = 0; i < blkInts.Count; i++)
	{	
		if (blkInts[i].Type.Contains("Cargo") && blkInts[i].Name.Contains('!'))
		{
			for (var label = Labeling.Ore; label < Labeling.NumOfLabels; label++)
			{
				if (CheckForLabel(blkInts[i].Name, label.ToString()))
				{
					blkInts[i].labels[(int)label] = true;
				}
			}
		}
	}
	//WriteLine(debugScreen, "Labels done");
	
	// Count
	for (int i = 0; i < blkInts.Count; i++)
	{
		// FIX: Input only
		var items = blkInts[i].Input.GetItems();
		for (int j = 0; j < items.Count; j++)
		{
			//WriteLine(debugScreen, ".Content: " + items[j].Content.ToString().Substring(27) + " .Sub: " + items[j].Content.SubtypeName);
			for (var label = Labeling.Ore; label < Labeling.NumOfLabels; label++)
			{
				if (items[j].Content.ToString().Contains(label.ToString()) && items[j].Content.SubtypeName != "Ice")
				{
					if (label == Labeling.Component)
					{
						components[items[j].Content.SubtypeName].Now += (double)items[j].Amount;
					}
					else
					{
						MaterialType t = (MaterialType) Enum.Parse(typeof(MaterialType), items[j].Content.SubtypeName.ToString());
						counters[(int)label, (int)t] += (double)items[j].Amount;
					}
					
				}
			}
		}
	}
	//WriteLine(debugScreen, "Count done");
	
	// Search for containers
	// TODO: Priority
	for (int i = 0; i < blkInts.Count; i++)
	{
		if (blkInts[i].Type.Contains("Cargo Container"))
		{
			for (var label = Labeling.Ore; label < Labeling.NumOfLabels; label++)
			{
				if (blkInts[i].labels[(int)label] && !blkInts[i].Input.IsFull)
				{
					if (containers[(int)label] == null)
					{
						containers[(int)label] = blkInts[i].Input;
					}
				}
			}
		}
	}
	//WriteLine(debugScreen, "Search done");
	
	// Basic sorting transfer
	for (int i = 0; i < blkInts.Count; i++)
	{
		var type = blkInts[i].Type;
		//WriteLine(debugScreen, "@ " + type);
		
		if (type == "Refinery")
		{
			var output = blkInts[i].Output;
			TransferAll(output, containers[(int)Labeling.Ingot]);
		}
		
		else if (type == "Assembler")
		{
			var output = blkInts[i].Output;
			TransferAll(output, containers[(int)Labeling.Component]);
		}
		
		else if (type.Contains("Cargo"))
		{
			var input = blkInts[i].Input;
			var items = input.GetItems();
			for (int j = 0; j < items.Count; j++)
			{
				for (var label = Labeling.Ore; label < Labeling.NumOfLabels; label++)
				{
					// If item is in the wrong place and is not ice
					if (!blkInts[i].labels[(int)label] && items[j].Content.ToString().Contains(label.ToString()) && items[j].Content.SubtypeName != "Ice")
					{
						WriteLine(debugScreen, "Move");
						input.TransferItemTo(containers[(int)label], j);
					}
				}
			}
		}
		
		else if (type.Contains("Reactor"))
		{
			var input = blkInts[i].Input;
			var items = input.GetItems();
			
			double uranium = 0;
			foreach (var item in items)
			{
				uranium += (double)item.Amount;
			}
			
			if (uranium < 1)
			{
				// Try find uranium in ingot container
				var ingots = containers[(int)Labeling.Ingot].GetItems();
				for (int j = 0; j < ingots.Count; j++)
				{
					if (ingots[j].Content.SubtypeName.Contains("Uranium"))
					{
						containers[(int)Labeling.Ingot].TransferItemTo(blkInts[i].Input, j, 0, true, 1);
					}
				}
			}
		}
		
		else if (type == "Oxygen Generator")
		{
			var input = blkInts[i].Input;
			var items = input.GetItems();
			double ice = 0;
			foreach (var item in items)
			{
				if (item.Content.SubtypeName == "Ice")
				{
					ice += (double)item.Amount;
				}
			}
			for (int j = 0; j < blkInts.Count; j++)
			{
				if (blkInts[j].Type.Contains("Cargo"))
				{
					var cInput = blkInts[j].Input;
					var cItems = cInput.GetItems();
					for (int k = 0; k < cItems.Count; k++)
					{
						// Get Ice
						if (ice < 5000 && cItems[k].Content.SubtypeName == "Ice")
						{
							cInput.TransferItemTo(input, k, 0, true, 5000);
						}
						// Move all bottles here
						// TODO: Move full bottles to a container?
						if (cItems[k].Content.SubtypeName.Contains("Bottle"))
						{
							cInput.TransferItemTo(input, k);
						}
					}
				}
			}
		}
		
		
	}
	//WriteLine(debugScreen, "Transfer done");
	
	// Auto-refining
	// Search for available refs
	BlockInterface refinery = null;
	for (int i = 0; i < blkInts.Count; i++)
	{
		if (blkInts[i].Type == "Refinery" && !((blkInts[i].Terminal as IMyRefinery).IsProducing))
		{
			refinery = blkInts[i];
		}
	}
	
	// Refine
	if (refinery != null)
	{
		WriteLine(debugScreen, "There is a free refinery, looking for ores to refine");
		
		// Check ingots
		for (var type = MaterialType.Stone; type < MaterialType.NumOfMaterialType; type++)
		{
			bool refStarted = false;
			if (counters[(int)Labeling.Ingot, (int)type] < refrates[type.ToString()] / 20 && counters[(int)Labeling.Ore, (int)type] > 0)
			{
				// Search for ore and transfer it
				var ores = containers[(int)Labeling.Ore].GetItems();
				for (int j = 0; j < ores.Count; j++)
				{
					if (ores[j].Content.SubtypeName == type.ToString())
					{
						// Start refining for 5 minutes at most
						containers[(int)Labeling.Ore].TransferItemTo(refinery.Input, j, 0, true, 
							refrates[type.ToString()] / 12
							);
						refStarted = true;
						break;
					}
				}
			}
			if (refStarted)
			{
				break;
			}
		}
	}
	
	// Look for new comps
	var comps = containers[(int)Labeling.Component].GetItems();
	for (int i = 0; i < comps.Count; i++)
	{
		string name = comps[i].Content.SubtypeName;
		if (!compItems.ContainsKey(name))
		{
			WriteLine(debugScreen, name + " is new! Added!");
			compItems.Add(name, MyDefinitionId.Parse("MyObjectBuilder_BlueprintDefinition/" + name));
		}
	}
	
	// Auto-assembling
	// Setup assemblers
	IMyAssembler mainAssembler = null;
	for (int i = 0; i < blkInts.Count; i++)
	{
		if (blkInts[i].Type == "Assembler" && CheckForLabel(blkInts[i].Name, ambMainLabel))
		{
			mainAssembler = blkInts[i].Terminal as IMyAssembler;
			break;
		}
	}
	
	if (mainAssembler == null)
	{
		WriteLine(debugScreen, "Error: Can't auto-assemble, there is no main assembler available!");
	}
	else if (mainAssembler.IsQueueEmpty)
	{
		// Assemble
		WriteLine(debugScreen, "Looking for missing components");
		
		// Check components
		foreach (var comp in components)
		{
			if (!comp.Value.Enough() && compItems.ContainsKey(comp.Key))
			{
				mainAssembler.AddQueueItem(compItems[comp.Key], (double)10);
				break;
			}
		}
	}
	
	// Ore Screen
	WriteLine(oreScreen, "--------- ORES ---------");
	for (MaterialType type = MaterialType.Stone; type < MaterialType.NumOfMaterialType; type++)
	{
		double amount = Math.Round(counters[(int)Labeling.Ore, (int)type]);
		WriteLine(oreScreen, type.ToString().PadRight(10) + amount.ToString().PadLeft(10) + " kg");
	}
	
	// Ingot Screen
	WriteLine(ingotScreen, "--------- INGOTS ---------");
	for (MaterialType type = MaterialType.Stone; type < MaterialType.NumOfMaterialType; type++)
	{
		double amount = Math.Round(counters[(int)Labeling.Ingot, (int)type]);
		Write(ingotScreen, type.ToString().PadRight(12));
		for (int i = 0; i < barSize; i++)
		{
			if ((double)i / barSize < amount / refrates[type.ToString()])
			{
				Write(ingotScreen, "|");
			}
			else
			{
				Write(ingotScreen, " ");
			}
		}
		Write(ingotScreen, amount.ToString().PadLeft(10) + " / " + refrates[type.ToString()] + " kg");
		WriteLine(ingotScreen, "");
	}
	
	WriteLine(compScreen, "--------- COMPONENTS ---------");
	foreach (var comp in components)
	{
		double amount = Math.Round(comp.Value.Now);
		Write(compScreen, comp.Key.PadRight(25));
		for (int i = 0; i < barSize; i++)
		{
			if ((double)i / barSize < amount / comp.Value.Req)
			{
				Write(compScreen, "|");
			}
			else
			{
				Write(compScreen, " ");
			}
		}
		Write(compScreen, amount.ToString().PadLeft(8) + " / " + comp.Value.Req);
		WriteLine(compScreen, "");
	}
	
	DateTime endTime = DateTime.Now;
	TimeSpan elapsedTime = (TimeSpan)(endTime - startTime);
	WriteLine(debugScreen, "Run #" + num++ + " Time: " + Math.Round(elapsedTime.TotalMilliseconds).ToString() + "ms");
}
