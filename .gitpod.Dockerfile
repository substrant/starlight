FROM gitpod/workspace-base

# Install dotnet
RUN sudo wget https://packages.microsoft.com/config/ubuntu/22.10/packages-microsoft-prod.deb -O /tmp/packages-microsoft-prod.deb && \
    sudo dpkg -i /tmp/packages-microsoft-prod.deb && \
    sudo rm /tmp/packages-microsoft-prod.deb && \
    sudo apt-get update && \
    sudo apt-get install -y dotnet-sdk-6.0