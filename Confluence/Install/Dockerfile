FROM mcr.microsoft.com/dotnet/sdk:8.0
RUN apt-get update && apt-get install -y git
RUN git clone https://github.com/TimKroeger13/GERMAG.git
RUN cd /GERMAG/ && git checkout 1-initial-database
RUN chmod -R 755 /GERMAG/
RUN /GERMAG/Confluence/Install/setup.sh
ENTRYPOINT [ "/bin/bash", "/GERMAG/Confluence/Install/run.sh" ]