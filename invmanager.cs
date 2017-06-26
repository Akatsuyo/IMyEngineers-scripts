// Display functions

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

//

public void GetInventories() {
	var blocks = new List<IMyTerminalBlock>();
	GridTerminalSystem.GetBlocks(blocks);
	foreach (var block in blocks)
	{
		if (block.HasInventory)
		{
			blkInts.Add(new BlockInterface(block));
		}
	}
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
		WriteLine(screens["Debug"], "Transferred: " + items[i].Amount + ' ' + items[i].Content.SubtypeName);
	}
}

public string GenerateID(IMyInventoryItem item)
{
	return item.Content.ToString().Substring(item.Content.ToString().IndexOf("_") + 1) + item.Content.SubtypeName;
}

public void addItem(IMyInventoryItem item) {
	string id = GenerateID(item);
	if (!allItems.ContainsKey(id))
	{
		// Add
		allItems.Add(id, new Item(item));
	}
	// Count
	allItems[id].Now += (double)item.Amount;
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

public class Item {
	private string type, sub;
	private double now, req;
	private MyDefinitionId blueprint;
	
	public Item(IMyInventoryItem item) {
		string tmpcontent = item.Content.ToString();
		type = tmpcontent.Substring(tmpcontent.IndexOf("_") + 1);
		sub = item.Content.SubtypeName;
		now = 0;
		
		if (type == "Component" || type == "Ingot")
		{
			switch (sub)
			{
				// Default values
				case "Stone":
					req = 370000;
					break;
				case "Iron":
					req = 260000;
					break;
				case "Silicon":
					req = 22000;
					break;
				case "Nickel":
					req = 4000;
					break;
				case "Cobalt":
					req = 1500;
					break;
				case "Silver":
					req = 2000;
					break;
				case "Gold":
					req = 500;
					break;
				case "Magnesium":
					req = 130;
					break;
				case "Uranium":
					req = 40;
					break;
				case "Platinum":
					req = 30;
					break;
				case "BulletproofGlass":
					req = 100;
					break;
				case "Computer":
					req = 500;
					break;
				case "Construction":
					req = 1000;
					break;
				case "Detector":
					req = 10;
					break;
				case "Display":
					req = 50;
					break;
				case "Explosives":
					req = 10;
					break;
				case "Girder":
					req = 100;
					break;
				case "GravityGenerator":
					req = 10;
					break;
				case "InteriorPlate":
					req = 500;
					break;
				case "LargeTube":
					req = 200;
					break;
				case "Medical":
					req = 10;
					break;
				case "MetalGrid":
					req = 200;
					break;
				case "Motor":
					req = 200;
					break;
				case "PowerCell":
					req = 10;
					break;
				case "RadioCommunication":
					req = 10;
					break;
				case "Reactor":
					req = 10;
					break;
				case "SmallTube":
					req = 200;
					break;
				case "SolarCell":
					req = 10;
					break;
				case "SteelPlate":
					req = 1000;
					break;
				case "Superconductor":
					req = 10;
					break;
				case "Thrust":
					req = 10;
					break;
				default:
					req = 0;
					break;
			}
		}
		else {
			req = 0;
		}
		
		string bpstr = "MyObjectBuilder_BlueprintDefinition/";
		switch (sub)
		{
			case "Computer":
			case "Construction":
			case "Detector":
			case "Explosives":
			case "Girder":
			case "GravityGenerator":
			case "Medical":
			case "Motor":
			case "RadioCommunication":
			case "Reactor":
			case "Thrust":
				bpstr += sub + "Component";
				break;
			default:
				bpstr += sub;
				break;
		}
		blueprint = MyDefinitionId.Parse(bpstr);
	}
	
	
	public string Type {
		get { return type; }
	}
	public string Sub {
		get { return sub; }
	}
	public double Now {
		get { return now; }
		set { now = value; }
	}
	public double Req {
		get { return req; }
		set { req = value; }
	}
	public bool HaveEnough() {
		return now >= req;
	}
	public MyDefinitionId Blueprint {
		get { return blueprint; }
		set { blueprint = value; }
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
static Dictionary<string, IMyTextPanel> screens = new Dictionary<string, IMyTextPanel>();
static List<BlockInterface> blkInts = new List<BlockInterface>();
static int barSize = 15;
static string ambMainLabel = "!main";
static Dictionary<string, int> refrates = new Dictionary<string, int>();
static Dictionary<string, Item> allItems = new Dictionary<string, Item>();


public Program() {
	screens.Add("Debug", GridTerminalSystem.GetBlockWithName("LCD Debug") as IMyTextPanel);
	screens.Add("Ore", GridTerminalSystem.GetBlockWithName("LCD Ores") as IMyTextPanel);
	screens.Add("Ingot", GridTerminalSystem.GetBlockWithName("LCD Ingots") as IMyTextPanel);
	screens.Add("Component", GridTerminalSystem.GetBlockWithName("LCD Components") as IMyTextPanel);
	WriteLine(screens["Debug"], "---------- IMyManager v0.1 ----------");
	GetInventories();
	// Refinery rates per hour
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
}

public void Save() {
}

public void Main() {
	// TODO List:
	// Scrap
	// Relative change
	// Critical ingot amount -> Try refine that
	// Auto-assemble check for ingots
	// Change requirements
	// GUI?
	// Priority
	// Handle scrap, ice
	// Different LCD sizes
	
	// Init	
	DateTime startTime = DateTime.Now;
	// Variables
	IMyInventory[] containers = new IMyInventory[(int)Labeling.NumOfLabels];
	
	// Reset
	foreach (var screen in screens)
	{
		Clear(screen.Value);
	}
	
	foreach (var item in allItems)
	{
		item.Value.Now = 0;
	}
	
	// Look for labels (!) and add / count items
	for (int i = 0; i < blkInts.Count; i++)
	{	
		if (blkInts[i].Type.Contains("Cargo") && blkInts[i].Name.Contains('!') && !blkInts[i].Input.IsFull)
		{
			for (var label = Labeling.Ore; label < Labeling.NumOfLabels; label++)
			{
				if (CheckForLabel(blkInts[i].Name, label.ToString()))
				{
					// Priority!
					blkInts[i].labels[(int)label] = true;
					containers[(int)label] = blkInts[i].Input;
				}
			}
		}
		
		// FIX: Input only
		var items = blkInts[i].Input.GetItems();
		for (int j = 0; j < items.Count; j++)
		{
			//WriteLine(screens["Debug"], ".Content: " + items[j].Content.ToString().Substring(27) + " .Sub: " + items[j].Content.SubtypeName);
			addItem(items[j]);
		}
		if (blkInts[i].HasOutput)
		{
			// Has output
			var outp = blkInts[i].Output;
			for (int j = 0; j < items.Count; j++)
			{
				addItem(items[j]);
			}
		}
	}
	
	// Basic sorting transfer
	for (int i = 0; i < blkInts.Count; i++)
	{
		var type = blkInts[i].Type;
		//WriteLine(screens["Debug"], "@ " + type);
		
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
					if (!blkInts[i].labels[(int)label] && items[j].Content.ToString().Contains(label.ToString()))
					{
						//WriteLine(screens["Debug"], "Move");
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
	//WriteLine(screens["Debug"], "Transfer done");
	
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
		WriteLine(screens["Debug"], "There is a free refinery, looking for ores to refine");
		
		// Check ingots
		foreach (var item in allItems.Values)
		{
			if (item.Type == "Ingot" && !item.HaveEnough())
			{
				WriteLine(screens["Debug"], "Need " + item.Sub);
				// Try refine this type of ore :S expensive task
				bool haveOre = false;
				bool refStarted = false;
				foreach (var subItem in allItems.Values)
				{
					if (subItem.Type == "Ore" && subItem.Sub == item.Sub && subItem.Now > 0)
					{
						haveOre = true;
						break;
					}
				}
				if (haveOre)
				{
					for (int j = 0; j < blkInts.Count; j++)
					{
						if (blkInts[j].labels[(int)Labeling.Ore])
						{
							var ores = blkInts[j].Input.GetItems();
							for (int k = 0; k < ores.Count; k++)
							{
								if (ores[k].Content.SubtypeName == item.Sub)
								{
									// Start refining for 5 minutes at most
									blkInts[j].Input.TransferItemTo(refinery.Input, k, 0, true, refrates[item.Sub] / 12);
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
				if (refStarted)
				{
					break;
				}
			}
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
		WriteLine(screens["Debug"], "Error: Can't auto-assemble, there is no main assembler available!");
	}
	else if (mainAssembler.IsQueueEmpty)
	{
		// Assemble
		WriteLine(screens["Debug"], "Looking for missing components");
		
		// Check components
		foreach (var item in allItems.Values)
		{
			if (item.Type == "Component" && !item.HaveEnough())
			{
				mainAssembler.AddQueueItem(item.Blueprint, (double)10);
				break;
			}
		}
	}
	
	// Screens
	WriteLine(screens["Ore"], "--------- ORES ---------");
	WriteLine(screens["Ingot"], "--------- INGOTS ---------");
	WriteLine(screens["Component"], "--------- COMPONENTS ---------");
	foreach (var item in allItems.Values)
	{
		if (item.Type == "Ore")
		{
			WriteLine(screens[item.Type], item.Sub.ToString().PadRight(10) + Math.Round(item.Now).ToString().PadLeft(10) + " kg");
		}
		else if (item.Type == "Ingot" || item.Type == "Component")
		{
			Write(screens[item.Type], item.Sub.ToString().PadRight(20));
			for (int i = 0; i < barSize; i++)
			{
				if ((double)i / barSize < item.Now / item.Req)
				{
					Write(screens[item.Type], "|");
				}
				else
				{
					Write(screens[item.Type], " ");
				}
			}
			Write(screens[item.Type], Math.Round(item.Now).ToString().PadLeft(10) + " / " + item.Req);
			WriteLine(screens[item.Type], "");
		}
	}
	
	DateTime endTime = DateTime.Now;
	TimeSpan elapsedTime = (TimeSpan)(endTime - startTime);
	WriteLine(screens["Debug"], "Run #" + num++ + " Time: " + Math.Round(elapsedTime.TotalMilliseconds).ToString() + "ms");
}
