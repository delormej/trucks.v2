#!/bin/bash
###############################################################################
#
# Use this to build locally and do a canary deploy instead of Cloud Build.
#
# Optionally provide the argument `local` to avoid pushing to Cloud Run.
#
# ./build.sh local
#
###############################################################################
set -e

LOCAL=$1

PROJECT_ID=$(gcloud config list --format 'value(core.project)')
REGION=$(gcloud config list --format 'value(run.region)')
SUFFIX=$(git rev-parse --short HEAD)

if [ -z $PROJECT_ID ]
then
  echo "ERROR.  Please ensure that your core.project is set with gcloud config."
  exit
fi

if [ -z $REGION ] 
then
  echo "ERROR.  Please ensure that your run/region, for example: gcloud config set run/region us-central1"
  exit
fi

IMAGE="$REGION-docker.pkg.dev/$PROJECT_ID/trucks-api/trucks-api:$SUFFIX"

docker build \
  --rm \
  --build-arg COMMIT_SHA=${SUFFIX} \
  -t $IMAGE \
  .

# Only build if local dev specified.
if [ "$LOCAL" = "local" ]; then
  exit
fi

# Send to Cloud Build to Build & Deploy
#SHORT_SHA=$(git rev-parse --short HEAD)
# gcloud builds submit --substitutions=SHORT_SHA=$SHORT_SHA

# Alternatively, to manually deploy to Cloud Run:
docker push $IMAGE

gcloud run deploy \
        --platform managed \
        --allow-unauthenticated \
        --service-account=trucks-sa@jasondel-truck.iam.gserviceaccount.com \
        --region us-central1 \
        --image $IMAGE \
        trucks-api
#        --no-traffic \

# #To restore sending traffic to new builds: 
# gcloud run services update-traffic --to-latest trucks-api
