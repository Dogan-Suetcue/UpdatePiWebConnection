# Update PiWeb Connection

This application can be used to automatically update the database connection for multiple PiWeb reports.

## Description
Update PiWeb Connection is a WPF application that allows users to easily update the database connection settings for PiWeb reports. The application checks for necessary prerequisites and provides various authentication modes for user selection.

## Features
- Checks for the installation of PiWeb and Cmdmon.exe
- Supports multiple authentication methods
- User-friendly interface for selecting report folders
- Progress indication during the update process

## Download
### For End Users
You can download the latest executable version of the application from the [Releases](https://github.com/Dogan-Suetcue/UpdatePiWebConnection/releases) section of this repository.

### For Developers
To view the source code, clone the repository:
```bash
git clone https://github.com/Dogan-Suetcue/UpdatePiWebConnection.git
```
Then, open the project in Visual Studio and build it to run the application.

## Screenshot
![Screenshot of the application](https://github.com/Dogan-Suetcue/UpdatePiWebConnection/blob/master/UpdatePiWebConnection.UI/Images/app.png)

## Usage
There are two ways to use the application:

1. **Default Database Connection**: 
   - If the `Default Database Connection` checkbox is checked (default value is true), you only need to define the path to the folder where the PiWeb reports are saved. The application will automatically use the default database connection settings.

2. **Custom Database Connection**: 
   - If the `Default Database Connection` checkbox is unchecked, you must define the following settings:
     - Select the authentication mode.
     - Enter the database name.
     - Type in the URL of the PiWeb server.
3. After defining these settings, you can proceed by pressing the Update button to apply the changes.

## License

[MIT](https://choosealicense.com/licenses/mit/)

## Contact
For questions or suggestions, you can reach me at suetcue.dogan@gmail.com
