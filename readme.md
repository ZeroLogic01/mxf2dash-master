# The MXF2Dash Converter

The following is an explanation on how to use the MXF2Dash solution

## The Slave application
    
The slave application is installed on each machine you want the transfers to happens on. It does not require special configuration files.
At start-up, the slave application will begin to listen on an IP selected from either the Wireless Network interface, or the Ethernet NI, and will prin a message with the same ip, stating that it awaits negociation.

After negociation, it will begin to listen for incoming connexions. After receiving a connexion, it will start to copy the file to the input folder, then start building the FFMPEG command, first probing the file for an audio stream then for the resolution, and then finally running the FFMPEG utilitary.

The slave will process at most a number of files equal to the *MaxWork* parameter gotten from the master, and output the results in the output folder

## The Master application

The Master application is installed only on the machine that contains the *watchfolder*. Before starting the application make sure to configure the **config.json** file, otherwise you will get an error. Below is the config file template.

Please note that: 
1. The delay is an integer number in seconds
2. The Command object represents the command that will be run on the slave. The *Type* was added so that it could be extended in more ways, as ulterior development
3. The command text has the following placeholders:
    3.1. **{in}** - represents the input folder of the slave
    3.2. **{out}** - represents the output file name of the slave
    3.3. **{audio}** - represents the result of the audio probe
    3.4. **{outDIR}** - represents the output directory of the slave
    3.5. **{resolution}** - represents the resolution of the resolution probe
4. The ConnectionPort number is for the Master to know on which port runs each Slave. The Slaves are hardcoded to that port.
5. If the master cannot connect to a certain Slave, or a folder is marked as invalid on the slave, it will not send Data to that slave.
6. The delay represents the delay between adding the file from the watchfolder to the Sending queue.




    {
    "Watchfolder": "Path to the Watchfolder",
    "Delay": delayIntegerInSeconds,
    "Command":{
        "type":"BASE",
        "text":"the command that ffmpeg will run"
    },
    "ConnectionPort": 5001,
    "Slaves": [
        {
            "MaxWork": maxAmountOfFilesThisSlaveCanProcess1,
            "InputFolder": "SlaveInputTempFolder1",
            "OutputFolder": "SlaveOutputTempFolder1",
            "IP": "slave1IPAsString"
        }
        {
            "MaxWork": maxAmountOfFilesThisSlaveCanProcess2,
            "InputFolder": "SlaveInputTempFolder2",
            "OutputFolder": "SlaveOutputTempFolder2",
            "IP": "slave2IPAsString"
        }
        {
            "MaxWork": maxAmountOfFilesThisSlaveCanProcess3,
            "InputFolder": "SlaveInputTempFolder3",
            "OutputFolder": "SlaveOutputTempFolder3",
            "IP": "slave3IPAsString"
        }
    ]
    }

## Commands
Each application can receive commands. Those commands are implemented throught a combination of the Strategy Design Pattern and Factory Design Pattern.
The Slave has no commands implemented
For a list of the implemented commands on the Master, please use the ***help*** command
