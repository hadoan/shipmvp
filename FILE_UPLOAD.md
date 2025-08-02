# File Upload System

This implementation provides a complete file upload system using Google Cloud Storage (GCP) instead of AWS S3, following ABP framework patterns.

## Features

- **File Upload**: Upload single or multiple files via drag-and-drop or file picker
- **GCP Storage**: Uses Google Cloud Storage as the backend storage service
- **Entity Events**: Publishes events when files are created/deleted using the ABP-style event system
- **File Management**: List, download, and delete files with metadata tracking
- **Public/Private Files**: Support for both public and private file access
- **Signed URLs**: Generate temporary signed URLs for secure file access
- **File Validation**: File size limits and type validation
- **Responsive UI**: Modern React frontend with drag-and-drop support

## Architecture

### Domain Layer (`ShipMvp.Domain.Files`)
- **File Entity**: Core domain entity representing uploaded files
- **IFileRepository**: Repository interface for file data operations
- **IFileStorageService**: Service interface for cloud storage operations

### Application Layer (`ShipMvp.Application.Files`)
- **FileAppService**: Main application service for file operations
- **File DTOs**: Data transfer objects for API communication
- **Event Publishing**: Integrates with the local event bus

### Infrastructure Layer (`ShipMvp.Infrastructure.Files`)
- **FileRepository**: EF Core implementation of file repository
- **GcpFileStorageService**: Google Cloud Storage implementation
- **Database Configuration**: EF Core entity configuration

### API Layer (`ShipMvp.Api.Controllers`)
- **FilesController**: REST API endpoints for file operations

### Frontend (`frontend/src/pages/FileUploadDemo.tsx`)
- **React Component**: Modern UI with drag-and-drop support
- **File Management**: Upload, download, and delete operations
- **Responsive Design**: Mobile-friendly interface

## Setup Instructions

### 1. Google Cloud Storage Setup

1. Create a GCP project
2. Enable the Cloud Storage API
3. Create a service account with Storage Admin permissions
4. Download the service account key JSON file
5. Create a storage bucket

### 2. Configuration

Update `appsettings.Development.json`:

```json
{
  "GCP": {
    "ProjectId": "your-gcp-project-id",
    "Storage": {
      "DefaultBucket": "your-bucket-name",
      "CredentialsPath": "path/to/service-account-key.json"
    }
  }
}
```

### 3. Environment Variables (Production)

For production, use environment variables instead of hardcoded credentials:

```bash
GOOGLE_APPLICATION_CREDENTIALS=/path/to/service-account-key.json
GCP__ProjectId=your-project-id
GCP__Storage__DefaultBucket=your-bucket-name
```

### 4. Database Migration

Run the database migration to create the Files table:

```bash
cd backend/src/ShipMvp.Infrastructure
dotnet ef database update
```

## API Endpoints

### Upload File
```http
POST /api/files/upload
Content-Type: multipart/form-data

file: [file data]
isPublic: false
containerName: "optional-container-name"
tags: "optional,tags"
```

### List Files
```http
GET /api/files?page=1&pageSize=10&userId=guid&containerName=container
```

### Download File
```http
GET /api/files/{fileId}/download
```

### Get File Info
```http
GET /api/files/{fileId}
```

### Delete File
```http
DELETE /api/files/{fileId}
```

### Get Signed URL
```http
GET /api/files/{fileId}/signed-url?expirationHours=1
```

## Event System Integration

The file system publishes the following events:

- **EntityCreatedEventData<File>**: When a file is uploaded
- **EntityDeletedEventData<File>**: When a file is deleted

Example event handler:

```csharp
public class FileUploadedHandler : IEventHandler<EntityCreatedEventData<File>>
{
    public Task HandleAsync(EntityCreatedEventData<File> @event)
    {
        var file = @event.Entity;
        // Handle file upload (e.g., send notification, scan for viruses, etc.)
        return Task.CompletedTask;
    }
}
```

## Security Considerations

1. **File Size Limits**: Default 50MB limit (configurable)
2. **File Type Validation**: Validate MIME types
3. **Access Control**: User-based file access control
4. **Signed URLs**: Temporary access for secure file sharing
5. **Private Files**: Support for private files not accessible via public URLs

## Frontend Usage

Navigate to `/files` in the application to access the file upload interface.

Features:
- Drag and drop file upload
- Multiple file selection
- File list with metadata
- Download and delete operations
- Public URL access for public files

## GCP Storage Benefits

- **Scalability**: Handles large files and high throughput
- **Durability**: 99.999999999% (11 9's) annual durability
- **Global CDN**: Automatic global content distribution
- **Security**: IAM integration and encryption at rest
- **Cost-Effective**: Pay for what you use with multiple storage classes

## Development Notes

- The implementation follows ABP framework patterns
- Uses Entity Framework Core for data persistence
- Implements proper error handling and logging
- Supports both public and private file access modes
- Integrates with the existing event bus system

## Future Enhancements

1. **File Virus Scanning**: Integrate with security scanning services
2. **Image Processing**: Automatic thumbnail generation and image optimization
3. **File Versioning**: Support for file versions and history
4. **Bulk Operations**: Batch upload and delete operations
5. **File Categories**: Organize files into categories or folders
6. **Search**: Full-text search capabilities for file metadata
