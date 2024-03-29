# Guide: How to Setup UR PolyScope in WSL2

**Ubuntu version: 20.04 LTS**

**Polyscope version: ursim-5.13.0.113898**

For setting up WSL2, follow [this tutorial](https://www.youtube.com/watch?v=7KVdJ43dQmE&t=464s)

### Steps
1. Download the zip folder from [Uiversal Robot Official Site](https://www.universal-robots.com/download/software-e-series/simulator-linux/offline-simulator-e-series-ur-sim-for-linux-5130/).
2. Unzip it in Downloads Folder
3. Open the WSL2 user in VSCode
4. Open the `install.sh` file
    Copy & Paste the following bits into the file
    ```console
    #!/bin/bash
    userServiceDirectory() {
        echo "$URSIM_ROOT/service"
    }

    userDaemonManagerDirectory() {
        echo "/etc/runit/runsvdir-ursim-$VERSION"
    }

    installDaemonManager() {
        local userServiceDirectory=`userServiceDirectory`
        local userDaemonManagerDirectory=`userDaemonManagerDirectory`
        local userDaemonManagerRunScript="$userDaemonManagerDirectory/run"

        echo "installing limb32z1"	
        sudo apt-get -y install lib32z1

        echo "Installing daemon manager package"
        # if it fails comment out, and check answer https://askubuntu.com/a/665742
        sudo apt-get -y install runit

        echo "Creating user daemon service directory"
        sudo mkdir -p $userDaemonManagerDirectory
        echo '#!/bin/sh' | sudo tee $userDaemonManagerRunScript >/dev/null
        echo 'exec 2>&1' | sudo tee -a $userDaemonManagerRunScript >/dev/null
        echo "exec chpst -u`whoami` runsvdir $userServiceDirectory" | sudo tee -a $userDaemonManagerRunScript >/dev/null
        sudo chmod +x $userDaemonManagerRunScript

        echo "Starting user daemon service"
        sudo ln -sf $userDaemonManagerDirectory /etc/service/
        mkdir -p $userServiceDirectory
    }


    needToInstallJava() {
        echo "Checking java version"
        if command -v java; then
        # source https://stackoverflow.com/questions/7334754/correct-way-to-check-java-version-from-bash-script
            version=$(java -version 2>&1 | awk -F '"' '/version/ {print $2}')
            echo version "$version"
            if [[ "$version" > "1.6" ]]; then
            echo "java version accepted"
                return 0
        fi
        fi
        return 1
    }

    # if we are not running inside a terminal, make sure that we do
    tty -s
    if [[ $? -ne 0 ]]
    then
        xterm -e "$0"
        exit 0
    fi

    needToInstallJava
    if [[ $? -ne 0 ]]; then
        # install default jre for distribution, make sure that it's at least 1.6
        sudo apt-get -y install default-jre
        if [[ $? -ne 0 ]]; then
            echo "Failed installing java, exiting"
            exit 2
        fi
        needToInstallJava
        if [[ $? -ne 0 ]]; then
            echo "Installed java version is too old, exiting"
            exit 3
        fi
    fi

    set -e

    commonDependencies='libcurl4 libjava3d-* ttf-dejavu* fonts-ipafont fonts-baekmuk fonts-nanum fonts-arphic-uming fonts-arphic-ukai'
    if [[ $(getconf LONG_BIT) == "32" ]]
    then
            Dependencies_32='libxmlrpc-c++8v5'
        pkexec bash -c "apt-get -y install $commonDependencies $Dependencies_32"
    else
            #Note: since URController is essentially a 32-bit program
            #we have to add some 32 bit libraries, some of them picked up from the linux distribution
            #some of them are have been recompiled and are inside our ursim-dependencies directory in deb format
        packages=`ls $PWD/ursim-dependencies/*amd64.deb`
        # pkexec bash -c "apt-get -y install lib32gcc1 lib32stdc++6 libc6-i386 $commonDependencies && (echo '$packages' | xargs dpkg -i --force-overwrite)" 
        bash -c "sudo apt-get -y install lib32gcc1 lib32stdc++6 libc6-i386 $commonDependencies && (echo '$packages' | xargs sudo dpkg -i --force-overwrite)"
    fi

    source version.sh
    URSIM_ROOT=$(dirname $(readlink -f $0))

    echo "Install Daemon Manager"
    installDaemonManager

    for TYPE in UR3 UR5 UR10 UR16
    do
        FILE=$HOME/Desktop/ursim-$VERSION.$TYPE.desktop
        echo "[Desktop Entry]" > $FILE
        echo "Version=$VERSION" >> $FILE
        echo "Type=Application" >> $FILE
        echo "Terminal=false" >> $FILE
        echo "Name=ursim-$VERSION $TYPE" >> $FILE
        echo "Exec=${URSIM_ROOT}/start-ursim.sh $TYPE" >> $FILE
        echo "Icon=${URSIM_ROOT}/ursim-icon.png" >> $FILE
        chmod +x $FILE
    done

    pushd $URSIM_ROOT/lib &>/dev/null
    chmod +x ../URControl

    popd &>/dev/null

    ```
5. Run the following commands inside the `Downloads/ursim...`:
    `sudo apt install openjdk-8-jre`
    `sudo apt update`
6. Go to the `cd Downloads/ursim...` folder and run the following command `./install.sh`
7. To run the application run `bash ./start-ursim.sh`