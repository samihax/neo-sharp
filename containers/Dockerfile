## Builder stage
# use official Microsoft dotnet container
FROM microsoft/dotnet:latest as builder

# arguments to choose branch and source repo (defaults are development and CoZ/neo-sharp)
ARG BRANCH="development"
ARG SOURCE_REPO="https://github.com/CityOfZion/neo-sharp.git"

# create folders structure and clone neo-sharp repo
RUN mkdir /repo && \
    cd /repo && \
    git clone --recursive -b $BRANCH $SOURCE_REPO && \
    cd /repo/neo-sharp/src/NeoSharp.Application && \
    dotnet publish --configuration Release --output /home/neo-sharp

## Runtime stage
# use official Microsoft dotnet container
FROM microsoft/dotnet:2.1-runtime as runtime

# install rocksdb dependencies and delete temp packages after to clean the most garbase possible
RUN apt-get update && \
    apt-get install -y libgflags-dev libsnappy-dev zlib1g-dev libbz2-dev liblz4-dev libzstd-dev && \
    rm -rf /var/lib/apt/lists/* && \
    mkdir /home/neo-sharp

# copy app from builder stage
COPY --from=builder /home/neo-sharp/* /home/neo-sharp/

# workdir
WORKDIR /home/neo-sharp

EXPOSE 8000/tcp

# default first command to run when container is started will start app
CMD ["/usr/bin/dotnet", "NeoSharp.Application.dll"]
