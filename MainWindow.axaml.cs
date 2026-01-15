using System;
using System.Net.Sockets;
using System.Text;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace DoorPanels;

public partial class MainWindow : Window
{
    // Robot connection settings
    private const string IpAddress = "172.20.254.203";
    private const int UrscriptPort = 30002;
    private const int DashboardPort = 29999;

    // Simple state: which door are we on? (1..3)
    private int _nextDoorIndex = 1; // tracks which door should be moved next
    private int _lastDoorAtTable = 0; // remembers which door is currently at work table

    // Door 1: stack -> work table 
    private const string ScriptDoor1ToWorkTableTemplate = @"
def move_door1_to_work_table():
  global RPC = rpc_factory(""xmlrpc"", ""http://localhost:41414"")
  global TOOL_INDEX = 0

  def rg_is_busy():
    return RPC.rg_get_busy(TOOL_INDEX)
  end

  def rg_grip(width, force = 10):
    RPC.rg_grip(TOOL_INDEX, width + .0, force + .0)
    sleep(0.01)
    while (rg_is_busy()):
    end
  end

  p1  = p[.121, -.379, .043, 3.2, 0, 0]
  p2  = p[-.02718, -.5, -.0552, 3.175, -0, 0]
  p8  = p[-.02718, -.5, -.115, 3.175, -0, 0]
  p4  = p[.125, -.5607, -.0436, 0, -2.239, 2.175]  # work table

  movej(p1)
  rg_grip(80)                      # open gripper
  movej(p2)
  rg_grip(80)                      # ensure open before picking
  movel(p8)                        # go down to door 1
  rg_grip(__GRIP_WIDTH__, __GRIP_FORCE__)  # grip door (size-dependent)
  movel(p2)
  movej(p4)                        # move to work table and stop

end

move_door1_to_work_table()
";

    // Door 2: stack -> work table 
    private const string ScriptDoor2ToWorkTableTemplate = @"
def move_door2_to_work_table():
  global RPC = rpc_factory(""xmlrpc"", ""http://localhost:41414"")
  global TOOL_INDEX = 0

  def rg_is_busy():
    return RPC.rg_get_busy(TOOL_INDEX)
  end

  def rg_grip(width, force = 10):
    RPC.rg_grip(TOOL_INDEX, width + .0, force + .0)
    sleep(0.01)
    while (rg_is_busy()):
    end
  end

  p1  = p[.121, -.379, .043, 3.2, 0, 0]
  p2  = p[-.02718, -.5, -.0552, 3.175, -0, 0]
  p9  = p[-.02718, -.5, -.121, 3.175, -0, 0]
  p4  = p[.125, -.5607, -.0436, 0, -2.239, 2.175]

  movej(p1)
  rg_grip(80)                      # open gripper
  movej(p2)
  rg_grip(80)                      # ensure open
  movel(p9)                        # go down to door 2
  rg_grip(__GRIP_WIDTH__, __GRIP_FORCE__)  # grip door
  movel(p2)
  movej(p4)

end

move_door2_to_work_table()
";

    // Door 3: stack -> work table
    private const string ScriptDoor3ToWorkTableTemplate = @"
def move_door3_to_work_table():
  global RPC = rpc_factory(""xmlrpc"", ""http://localhost:41414"")
  global TOOL_INDEX = 0

  def rg_is_busy():
    return RPC.rg_get_busy(TOOL_INDEX)
  end

  def rg_grip(width, force = 10):
    RPC.rg_grip(TOOL_INDEX, width + .0, force + .0)
    sleep(0.01)
    while (rg_is_busy()):
    end
  end

  p1  = p[.121, -.379, .043, 3.2, 0, 0]
  p2  = p[-.02718, -.5, -.0552, 3.175, -0, 0]
  p3  = p[-.02718, -.5, -.132, 3.175, -0, 0]
  p4  = p[.125, -.5607, -.0436, 0, -2.239, 2.175]

  movej(p1)
  rg_grip(80)                      # open gripper
  movej(p2)
  rg_grip(80)                      # ensure open
  movel(p3)                        # go down to door 3
  rg_grip(__GRIP_WIDTH__, __GRIP_FORCE__)  # grip door
  movel(p2)
  movej(p4)

end

move_door3_to_work_table()
";

    // Door 1: work table -> storage
    private const string ScriptDoor1ToStorage = @"
def move_door1_to_storage():
  global RPC = rpc_factory(""xmlrpc"", ""http://localhost:41414"")
  global TOOL_INDEX = 0

  def rg_is_busy():
    return RPC.rg_get_busy(TOOL_INDEX)
  end

  def rg_grip(width, force = 10):
    RPC.rg_grip(TOOL_INDEX, width + .0, force + .0)
    sleep(0.01)
    while (rg_is_busy()):
    end
  end

  p1  = p[.121, -.379, .043, 3.2, 0, 0]
  p7  = p[.325, -.5607, -.0, 0.02, -2.186, 2.183]
  p6  = p[.325, -.566, -.092, 0.02, -2.186, 2.183]

  movej(p7)
  movel(p6)
  rg_grip(80)   # release door (open gripper)
  movej(p7)
  movej(p1)

end

move_door1_to_storage()
";

    // Door 2: work table -> storage
    private const string ScriptDoor2ToStorage = @"
def move_door2_to_storage():
  global RPC = rpc_factory(""xmlrpc"", ""http://localhost:41414"")
  global TOOL_INDEX = 0

  def rg_is_busy():
    return RPC.rg_get_busy(TOOL_INDEX)
  end

  def rg_grip(width, force = 10):
    RPC.rg_grip(TOOL_INDEX, width + .0, force + .0)
    sleep(0.01)
    while (rg_is_busy()):
    end
  end

  p1  = p[.121, -.379, .043, 3.2, 0, 0]
  p4  = p[.125, -.5607, -.0436, 0, -2.239, 2.175]
  p10 = p[.125, -.532, -.0436, 0, -2.239, 2.175]
  p11 = p[.325, -.532, -.0, 0.02, -2.186, 2.183]
  p12 = p[.325, -.532, -.097, 0.02, -2.186, 2.183]
  p13 = p[.325, -.532, -.0, 0.02, -2.186, 2.183]

  movej(p4)
  movel(p10)
  movel(p11)
  movel(p12)
  rg_grip(80)   # release door
  movel(p13)
  movej(p1)

end

move_door2_to_storage()
";

    // Door 3: work table -> storage
    private const string ScriptDoor3ToStorage = @"
def move_door3_to_storage():
  global RPC = rpc_factory(""xmlrpc"", ""http://localhost:41414"")
  global TOOL_INDEX = 0

  def rg_is_busy():
    return RPC.rg_get_busy(TOOL_INDEX)
  end

  def rg_grip(width, force = 10):
    RPC.rg_grip(TOOL_INDEX, width + .0, force + .0)
    sleep(0.01)
    while (rg_is_busy()):
    end
  end

  p1  = p[.121, -.379, .043, 3.2, 0, 0]
  p4  = p[.125, -.5607, -.0436, 0, -2.239, 2.175]
  p14 = p[.125, -.499, -.0436, 0, -2.239, 2.175]
  p15 = p[.325, -.499, -.0, 0.02, -2.186, 2.183]
  p16 = p[.325, -.499, -.097, 0.02, -2.186, 2.183]
  p17 = p[.325, -.499, -.0, 0.02, -2.186, 2.183]

  movej(p4)
  movel(p14)
  movel(p15)
  movel(p16)
  rg_grip(80)   # release door
  movel(p17)
  movej(p1)

end

move_door3_to_storage()
";

    private readonly string _username;
    private readonly AppDbContext _db = new(); // our database context
    private OrderLog? _currentOrder; // the order currently being worked on

    public MainWindow(string username)
    {
        _username = username;
        InitializeComponent();
    }

    // Helper: read selected door size
    private string GetSelectedDoorSize() // returns the currently selected door size from the ComboBox
    {
        if (DoorSizeComboBox?.SelectedItem is ComboBoxItem item) // check that ComboBox exists and selected item is a ComboBoxItem
            return item.Content?.ToString() ?? "Medium"; // Return selected item's text, default to "medium" if null

        return "Medium"; // fallback to "medium" if ComboBox or item is not valid
    }

    // Helper: map door size -> grip width (mm) + force
    private (int width, int force) GetGripParametersForSize() // computes grip width/force based on selected size
    {
        var size = GetSelectedDoorSize(); // get currently selected door size text

        // Mapping:
        // Small  = 4.8 cm  => 48 mm
        // Medium = 5.6 cm   => 56 mm
        // Large  = 6.4 cm   => 64 mm
        // Force is slightly increased with size.
        return size switch // pattern match on size string
        {
            "Small" => (47, 10),
            "Medium" => (56, 15),
            "Large" => (64, 20),
            _ => (48, 10) // default mapping if none matches
        };
    }

    // TCP (Transmission Control Protocol) helper
    private void SendString(string host, int port, string message) // sends a plain string to a given host/port using TCP
    {
        using var client = new TcpClient(host, port); // create a TCP client and connect to host/port (disposed automatically)

        if (!message.EndsWith("\n")) // ensure the message ends with newline as UR expects line termination
            message += "\n"; // append newline if missing

        using var stream = client.GetStream(); // get the network stream from the TCP client
        var bytes = Encoding.ASCII.GetBytes(message); // convert the message to ASCII bytes
        stream.Write(bytes, 0, bytes.Length); // send the bytes to the robot over the network
    }

    // Button handlers

    private async void BrakeRelease_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            SendString(IpAddress, DashboardPort, "brake release");

            // Start new order log
            var size = GetSelectedDoorSize();
            _currentOrder = new OrderLog // create a new OrderLog object that represents this session/order
            {
                Username = _username, // store which operator is currently logged in
                DoorSize = size, // store the selected door size (small/medium/large)
                StartedAt = DateTime.Now, // timestamp when operator clicked "brake release"
                FinishedAt = null // finish time sent when operator presses "finish"
            };

            _db.OrderLogs.Add(_currentOrder); // add new OrderLog entry to the SQLite database tracking set
            await _db.SaveChangesAsync(); // save the new record to the actual DB file on disk

            StatusText.Text = $"Status: Brake released. Order started by {_username}."; // update the GUI status text
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Error: {ex.Message}";
        }
    }

    private void MoveToWorkTable_Click(object? sender, RoutedEventArgs e) // event handler for "move door to work table" button
    {
        try // try to perform door move logic
        {
            if (_nextDoorIndex > 3) // if doors 1-3 are already processed
            {
                StatusText.Text = "Status: All doors processed."; // inform user no more doors in stack
                return; // exit handler early
            }

            var size = GetSelectedDoorSize(); // read selected door size from ComboBox
            var (gripWidth, gripForce) = GetGripParametersForSize(); // convert size to grip parameters

            // select the URScript template based on which door is next
            string template = _nextDoorIndex switch
            {
                1 => ScriptDoor1ToWorkTableTemplate, // use script for door 1
                2 => ScriptDoor2ToWorkTableTemplate, // use script for door 2
                3 => ScriptDoor3ToWorkTableTemplate, // use script for door 3
                _ => ScriptDoor1ToWorkTableTemplate // fallback (should not normally happen)
            };

            // Inject width + force into URScript
            string program = template // start from template script
                .Replace("__GRIP_WIDTH__", gripWidth.ToString()) // replace placeholder for grip width with actual value
                .Replace("__GRIP_FORCE__", gripForce.ToString()); // replace placeholder for grip force with actual value

            SendString(IpAddress, UrscriptPort, program); // send generated URScript program to robot URScript port

            _lastDoorAtTable = _nextDoorIndex; // remember which door is now at the work table
            StatusText.Text =
                $"Status: Door #{_lastDoorAtTable} at work table (size: {size}, width: {gripWidth} mm, force: {gripForce})."; // update UI with detailed status
        }
        catch (Exception ex) // catch any exceptions from networking or logic
        {
            StatusText.Text = $"Error: {ex.Message}"; // display error in status text
        }
    }

    private void MoveToStorage_Click(object? sender, RoutedEventArgs e) // even handler for "move door to storage magazine" button
    {
        try // try to perform move to storage
        {
            if (_lastDoorAtTable == 0) // if no door is currently at work table
            {
                StatusText.Text = "Status: No door at work table."; // inform user that there is nothing to move
                return; // exit handler
            }

            string program = _lastDoorAtTable switch // choose correct storage script based on last door at table
            {
                1 => ScriptDoor1ToStorage, // door 1 storage script
                2 => ScriptDoor2ToStorage, // door 2 storage script 
                3 => ScriptDoor3ToStorage, // door 3 storage script
                _ => ScriptDoor1ToStorage // fallback (should not occur)
            };

            SendString(IpAddress, UrscriptPort, program); // send chosen script to URScript port

            StatusText.Text = $"Status: Door #{_lastDoorAtTable} stored."; // tell user that door has been stored
            _nextDoorIndex++; // increase index so next door from stack is used next time
            _lastDoorAtTable = 0; // reset to indicate work table is now empty
        }
        catch (Exception ex) // catch any exception
        {
            StatusText.Text = $"Error: {ex.Message}"; // display error information
        }
    }

    private void Stop_Click(object? sender, RoutedEventArgs e) // event handler for "stop program" button
    {
        try // try to send stop command to robot
        {
            SendString(IpAddress, DashboardPort, "stop"); // send "stop" command to dashboard port
            StatusText.Text = "Status: Stop sent."; // confirm to user that stop was sent
        }
        catch (Exception ex) // handle connection or send errors
        {
            StatusText.Text = $"Error: {ex.Message}"; // show error in status text
        }
    }
    
    private void Logout_Click(object? sender, RoutedEventArgs e) // event handler for "logout" button
    {
        _nextDoorIndex = 1; // reset next door index back to first door
        _lastDoorAtTable = 0; // reset state so no door is at work table
        StatusText.Text = "Status: Logged out."; // inform user that logout is done

        this.Close(); // close the current MainWindow

        var login = new LoginWindow(); // create a new instance for the LoginWindow
        login.Show(); // show the login window again
    }
    
    private async void FinishOrder_Click(object? sender, RoutedEventArgs e) // event handler for "finish order" button
    {
        try // try to complete and log the order
        {
            if (_currentOrder == null) // check that an order was actually started first
            {
                StatusText.Text = "Status: No active order to finish."; // notify user that no order is in progress
                return; // exit early since there is nothing to finish
            }

            _currentOrder.FinishedAt = DateTime.Now; // timestamp when operator completed the order
            await _db.SaveChangesAsync(); // update existing OrderLog row in SQLite DB with finish time

            StatusText.Text = $"Status: Order finished by {_username}."; // inform user that order has been logged
            _currentOrder = null; // clear active order reference so a new one can be started next time
        }
        catch (Exception ex) // catch any unexpected failures (DB or UI)
        {
            StatusText.Text = $"Error: {ex.Message}"; // show error message in status text
        }
    }
}



