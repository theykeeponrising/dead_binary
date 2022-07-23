import os
from os.path import dirname, abspath
cwd = dirname(abspath(__file__))

os.chdir(cwd)

with open(r"Models\soldier.fbx.meta", "r+") as openfile:
    lines = openfile.readlines()
    newlines = []

    for line in lines:
        if "loopTime: 0" in line:
            line = line.replace("0", "1")
        elif "loopBlendOrientation: 0" in line:
            line = line.replace("0", "1")
        elif "loopBlendPositionY: 0" in line:
            line = line.replace("0", "1")
        elif "loopBlendPositionXZ: 0" in line:
            line = line.replace("0", "1")
        elif "keepOriginalOrientation: 0" in line:
            line = line.replace("0", "1")
        elif "keepOriginalPositionY: 0" in line:
            line = line.replace("0", "1")
        elif "keepOriginalPositionXZ: 0" in line:
            line = line.replace("0", "1")

        newlines.append(line)

with open(r"Models\soldier.fbx.meta", "w+") as openfile:
    openfile.writelines(newlines)
