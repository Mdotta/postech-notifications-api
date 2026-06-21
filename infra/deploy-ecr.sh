#!/bin/bash
set -euo pipefail

# =============================================================================
# deploy-ecr.sh
# Builds the Lambda container image for linux/amd64, authenticates with AWS ECR,
# and pushes the image.
#
# Usage:
#   ./infra/deploy-ecr.sh
#
# Required environment variables:
#   AWS_ACCOUNT_ID  - Your AWS account ID (e.g. 123456789012)
#
# Optional environment variables:
#   AWS_REGION      - AWS region where ECR lives (default: us-east-1)
#   ECR_REPO        - ECR repository name (default: tf-postech-notifications-lambda)
#   IMAGE_TAG       - Image tag (default: latest)
# =============================================================================

AWS_ACCOUNT_ID="${AWS_ACCOUNT_ID:?❌ AWS_ACCOUNT_ID is not set}"
AWS_REGION="${AWS_REGION:-us-east-1}"
ECR_REPO="${ECR_REPO:-tf-postech-notifications-lambda}"
IMAGE_TAG="${IMAGE_TAG:-latest}"
PLATFORM="linux/amd64"

ECR_REGISTRY="$AWS_ACCOUNT_ID.dkr.ecr.$AWS_REGION.amazonaws.com"
FULL_IMAGE="$ECR_REGISTRY/$ECR_REPO:$IMAGE_TAG"

log()  { echo "[$(date '+%H:%M:%S')] $*"; }
ok()   { echo "[$(date '+%H:%M:%S')] ✅ $*"; }
fail() { echo "[$(date '+%H:%M:%S')] ❌ $*" >&2; exit 1; }

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
LAMBDA_DIR="$SCRIPT_DIR/../src/Postech.Notifications.Lambda"
cd "$LAMBDA_DIR"

log "Checking dependencies..."
command -v docker &>/dev/null || fail "docker is not installed"
command -v aws    &>/dev/null || fail "aws CLI is not installed"
[[ -f "Dockerfile" ]] || fail "Dockerfile not found at $LAMBDA_DIR"

# --- Step 1: Ensure buildx builder is available ------------------------------
log "Setting up Docker buildx for $PLATFORM..."
if ! docker buildx inspect postech-builder &>/dev/null; then
  docker buildx create --name postech-builder --use
  ok "buildx builder 'postech-builder' created"
else
  docker buildx use postech-builder
  log "Reusing existing buildx builder 'postech-builder'"
fi

# --- Step 2: Authenticate with ECR -------------------------------------------
log "Authenticating Docker with ECR ($ECR_REGISTRY)..."
aws ecr get-login-password --region "$AWS_REGION" | \
  docker login --username AWS --password-stdin "$ECR_REGISTRY"
ok "Authenticated with ECR"

# --- Step 3: Verify ECR repository exists (created by Terraform) ---------------
log "Verifying ECR repository '$ECR_REPO' exists..."
aws ecr describe-repositories --repository-names "$ECR_REPO" --region "$AWS_REGION" &>/dev/null || \
  fail "ECR repository '$ECR_REPO' not found. Run 'terraform apply' first to create it."
ok "ECR repository ready: $ECR_REPO"

# --- Step 4: Build and push --------------------------------------------------
log "Building image for $PLATFORM..."
docker buildx build \
  --platform "$PLATFORM" \
  --load \
  -t "$FULL_IMAGE" \
  .
ok "Image built: $FULL_IMAGE"

log "Pushing image to ECR..."
docker push "$FULL_IMAGE"
ok "Image pushed: $FULL_IMAGE"

echo ""
echo "🚀 ECR deploy complete!"
echo "   Image    : $FULL_IMAGE"
echo "   Platform : $PLATFORM"
