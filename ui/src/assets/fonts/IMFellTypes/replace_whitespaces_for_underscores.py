
import os

def replace_spaces_with_underscores(directory):
    # Get a list of all files in the specified directory
    files = os.listdir(directory)

    # Iterate through each file in the directory
    for file_name in files:
        # Check if the file is a regular file (not a directory)
        if os.path.isfile(os.path.join(directory, file_name)):
            # Replace spaces with underscores in the filename
            new_file_name = file_name.replace(' ', '_')

            # Construct the full path for the old and new filenames
            old_path = os.path.join(directory, file_name)
            new_path = os.path.join(directory, new_file_name)

            # Rename the file
            os.rename(old_path, new_path)
            print(f'Renamed: {old_path} to {new_path}')

# Get the current working directory of the script
script_directory = os.path.dirname(os.path.realpath(__file__))

# Call the function to replace spaces with underscores
replace_spaces_with_underscores(script_directory)