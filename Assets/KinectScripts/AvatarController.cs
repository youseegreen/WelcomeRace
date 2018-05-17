using UnityEngine;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using System.Text; 

//offsetNode  ��̃Q�[���I�u�W�F�N�g������Ă����Ƀv���C���[�Ԃ�����ł���
//�@�@�@�@�@�@�����ɂ���āA�v���C���[�̈ʒu�����_�ɂ���Ƃ��čl������  �L���������_�Ƃ������W�ɕς��Ă��邾��
//bodyRoot�@  unity���E���W��ł̃L�����̑匳��transform  �L�����ɕt�����ē����Ă�

[RequireComponent(typeof(Animator))]    //�A�j���[�^�[�����鎞�������̂��ȁH
public class AvatarController : MonoBehaviour
{	
	// Bool that has the characters (facing the player) actions become mirrored. Default false.
	public bool mirroredMovement = false;
	
	// Bool that determines whether the avatar is allowed to move in vertical direction.
	public bool verticalMovement = false;
	
	// Rate at which avatar will move through the scene. The rate multiplies the movement speed (.001f, i.e dividing by 1000, unity's framerate).
	protected float moveRate = 4.0f; //int��float�ɕς����@���s���̊֐��ɂ����ق����悳����
	
	// Slerp smooth factor
	public float smoothFactor = 5f;
	
	// Whether the offset node must be repositioned to the user's coordinates, as reported by the sensor or not.
	public bool offsetRelativeToSensor = false;
	

	// The body root node
	protected Transform bodyRoot;
	
	// A required variable if you want to rotate the model in space.
	protected GameObject offsetNode;
	
	// Variable to hold all them bones. It will initialize the same size as initialRotations.
	protected Transform[] bones;    //�����̒��g�Ƃ��Ă����

    public Transform[] Bones
    {
        get { return this.bones; }
    }
	
	// Rotations of the bones when the Kinect tracking starts.
	protected Quaternion[] initialRotations;
	protected Quaternion[] initialLocalRotations;
	
	// Initial position and rotation of the transform
	protected Vector3 initialPosition;
	protected Quaternion initialRotation;
	
	// Calibration Offset Variables for Character Position.
	protected bool offsetCalibrated = false;
	protected float xOffset, yOffset, zOffset;

	// private instance of the KinectManager
	protected KinectManager kinectManager;


	// transform caching gives performance boost since Unity calls GetComponent<Transform>() each time you call transform 
	private Transform _transformCache;
	public new Transform transform
	{
		get
		{
			if (!_transformCache) 
				_transformCache = base.transform;
			
			return _transformCache;
		}
	}

    //unity�����̃{�[���̈ʒu��bones
    //  "  �p�x�Ƃ���initialRotations�@�͂��߂����₩��ʂɂ��邩��


    //�{�[���������ŃZ�b�g���Ă���!
    public void Awake()
    {	
		// check for double start
		if(bones != null)
			return;

        //�̈�m��
		// inits the bones array
		bones = new Transform[22];  //22�{�[��
		// Initial rotations and directions of the bones.
		initialRotations = new Quaternion[bones.Length];
		initialLocalRotations = new Quaternion[bones.Length];
        
        //�Ƃ肠����������unity�����̃{�[�����E���Ă��Ă���
		// Map bones to the points the Kinect tracks
		MapBones();

        //unity�����̃{�[���̈ʒu����p�x���v�Z���Ă���
		// Get initial bone rotations
		GetInitialRotations();
	}



    // Update the avatar each frame.
    /*�S�̂̈ʒu���e�p�[�c�̈ʒu�@�@���ė���*/
    /*bodyRoot�������ׂĂ���*/
    public void UpdateAvatar(uint UserID)
    {
        if (!transform.gameObject.activeInHierarchy)
        {     //�L�������A�N�e�B�u����Ȃ��Ȃ烊�^�[��
            return;
        }
		// Get the KinectManager instance
		if(kinectManager == null)
		{
			kinectManager = KinectManager.Instance;
		}
		
        /*�����Ł@�J�����̉�(UserID)���ǂ������������Q�b�g���Ă���͂�*/
        /*���S�̂̈ʒu���v�Z��unity�����̈ʒu���v�Z�H*/
        /*�ʒu�����i�p���͓����ĂȂ��j*/
		// move the avatar to its Kinect position
		MoveAvatar(UserID);

        
        /*�S�̂̈ʒu�����܂����と�֐߂Ƃ��̈ʒu�����߂Ă����Ă���*/
        /*unity�����𓮂����Ă���*/
        //��������{�[���̏��Q�b�g�ł�����
        
		for (var boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
			if (!bones[boneIndex])  //�{�[����񂪂Ȃ��Ƃ��̓p�X
				continue;
			
            //���̕ӂ̓��e����΂��Ă���
			if(boneIndex2JointMap.ContainsKey(boneIndex))
			{
				KinectWrapper.NuiSkeletonPositionIndex joint = !mirroredMovement ? boneIndex2JointMap[boneIndex] : boneIndex2MirrorJointMap[boneIndex];

                //�{�[���擮�����Ă�@�Ȃ��UserID���������
                TransformBone(UserID, joint, boneIndex, !mirroredMovement);
			}

            //���������͗��Ă��Ȃ��̂���
			else if(specIndex2JointMap.ContainsKey(boneIndex))
			{
				// special bones (clavicles)
				List<KinectWrapper.NuiSkeletonPositionIndex> alJoints = !mirroredMovement ? specIndex2JointMap[boneIndex] : specIndex2MirrorJointMap[boneIndex];
				
				if(alJoints.Count >= 2)
				{
					//Vector3 baseDir = alJoints[0].ToString().EndsWith("Left") ? Vector3.left : Vector3.right;
					//TransformSpecialBone(UserID, alJoints[0], alJoints[1], boneIndex, baseDir, !mirroredMovement);
				}
			}
		}



        //�����Ƀ{�[���ƈʒu����͂��āA�Z�o�Ă邩���ʂ���@�\����
	}
	


    /*�ʒu�͖߂���ق��������񂩂�*/
    /*���[�U�[��mia�����Ƃ��ɌĂ΂��̂���?�Ƃ肠����������Ԃɖ߂��Ă���ۂ�*/
	// Set bones to their initial positions and rotations
	public void ResetToInitialPosition()
	{	
		if(bones == null)
			return;
		
        //�S�̂̎p����߂�
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = Quaternion.identity;
		}
		else
		{
			transform.rotation = Quaternion.identity;
		}
		
        //�֐߂̎p����߂�
		// For each bone that was defined, reset to initial position.
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				bones[i].rotation = initialRotations[i];
			}
		}
		
        /*�����������̍���*/
        
		if(bodyRoot != null)
		{
			bodyRoot.localPosition = Vector3.zero;
			bodyRoot.localRotation = Quaternion.identity;
		}
        
		// Restore the offset's position and rotation
		if(offsetNode != null)
		{
			offsetNode.transform.position = initialPosition;
			offsetNode.transform.rotation = initialRotation;
		}
		else
		{
			transform.position = initialPosition;
			transform.rotation = initialRotation;
		}
	}
	

    //���[�U�[�̃L�����u���[�V�����H  �v����
	// Invoked on the successful calibration of a player.
	public void SuccessfulCalibration(uint userId)
	{
		// reset the models position
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = initialRotation;
		}
		
		// re-calibrate the position offset
		offsetCalibrated = false;
	}

	

    /*�����͂߂�����厖����*/
    /*�{�[���𓮂����Ă邯�ǁA�ǂ��ŎQ�Ƃ��Ă���̂��
      boneTransform��bones[index]�̎Q�ƂɂȂ��Ă���񂩂ȁH�@���ƂŒ��ׂ�*/
    /*���̏ꍇ������unity�����̃{�[���̊p�x(�ʒu���H)�ς��Ă�*/
	// Apply the rotations tracked by kinect to the joints.
	protected void TransformBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, int boneIndex, bool flip)
    {
		Transform boneTransform = bones[boneIndex]; //unity�����̃{�[���̈ʒu�Ƃ������
        if (boneTransform == null || kinectManager == null)  //�{�[���Ȃ�������kinect���ĂȂ�������p�X
			return;
		
		int iJoint = (int)joint;    //�֐ߐ��������@�@�ϊ��ł��񂩂�����|�Ȃ�񂩂ȁH
		if(iJoint < 0)
			return;


        /*�������烁�C��*/
        //�����Ń��[�U�[�̂���֐߂̊p�x�Q�b�g���Ă�?
        Quaternion jointRotation = kinectManager.GetJointOrientation(userId, iJoint, flip);
        //�擾�ł��Ȃ�������return
		if(jointRotation == Quaternion.identity)
			return;
        //�����Ń��[�U�[�̂������̏��g���ă��[�U�[�̉�]�p�x�Q�b�g���Ă�
        // Smoothly transition to the new rotation
        Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);

        //�O�̃{�[�����獡�̃{�[���Ɉڂ�܂ł̕⊮���@�@smoothFactor��0�ȊO�Ȃ�v�Z���Ԃ����邯�ǃX���[�Y�ɓ���
		if(smoothFactor != 0f)
        	boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
        //�Ȃ��Ȃ狭���ɐV�����ʒu�ɂ�����
        else
			boneTransform.rotation = newRotation;
	}


	/*���ʂȊ֐߂���΂������������Ă�@�@��{�I�ȗp�r�͏�̂Ɠ���*/
	// Apply the rotations tracked by kinect to a special joint
	protected void TransformSpecialBone(uint userId, KinectWrapper.NuiSkeletonPositionIndex joint, KinectWrapper.NuiSkeletonPositionIndex jointParent, int boneIndex, Vector3 baseDir, bool flip)
	{
		Transform boneTransform = bones[boneIndex];
		if(boneTransform == null || kinectManager == null)
			return;
		
		if(!kinectManager.IsJointTracked(userId, (int)joint) || 
		   !kinectManager.IsJointTracked(userId, (int)jointParent))
		{
			return;
		}
		
		Vector3 jointDir = kinectManager.GetDirectionBetweenJoints(userId, (int)jointParent, (int)joint, false, true);
		Quaternion jointRotation = jointDir != Vector3.zero ? Quaternion.FromToRotation(baseDir, jointDir) : Quaternion.identity;
		
//		if(!flip)
//		{
//			Vector3 mirroredAngles = jointRotation.eulerAngles;
//			mirroredAngles.y = -mirroredAngles.y;
//			mirroredAngles.z = -mirroredAngles.z;
//			
//			jointRotation = Quaternion.Euler(mirroredAngles);
//		}
		
		if(jointRotation != Quaternion.identity)
		{
			// Smoothly transition to the new rotation
			Quaternion newRotation = Kinect2AvatarRot(jointRotation, boneIndex);
			
			if(smoothFactor != 0f)
				boneTransform.rotation = Quaternion.Slerp(boneTransform.rotation, newRotation, smoothFactor * Time.deltaTime);
			else
				boneTransform.rotation = newRotation;
		}
		
	}
	


    /*���������C��*/
    /*kinect�J�������l�̈ʒu��unity��ł�unity�����̍��W���v�Z���Ă���*/
	// Moves the avatar in 3D space - pulls the tracked position of the spine and applies it to root.
	// Only pulls positional, not rotational.
	protected void MoveAvatar(uint UserID)
	{
        if (bodyRoot == null || kinectManager == null)   //body��Transform�̐e��
			return;
        //�L�l�N�g��UserID���g���b�L���O�ł��Ă��Ȃ���΃p�X
        if (!kinectManager.IsJointTracked(UserID, (int)KinectWrapper.NuiSkeletonPositionIndex.HipCenter))
			return;

        /*trans��UserID�̐l�̍��W������Ă���*/
        // Get the position of the body and store it.
        Vector3 trans = kinectManager.GetUserPosition(UserID);


        /*���ڂ����L�����u���[�V�������Ă�*/
        /*xyzOffset��unity��ł̏������W���Q�b�g���Ă���*/
        /*����kinect�̍��W�����w�̍��W�Ɠ����Łi��ʍ��Ȃ�[�jmoveRate�͎���Ԃ�unity��Ԃ̔{���ɂȂ��Ă�*/
        // If this is the first time we're moving the avatar, set the offset. Otherwise ignore it.
        if (!offsetCalibrated)
		{
			offsetCalibrated = true;
			
			xOffset = !mirroredMovement ? trans.x * moveRate : -trans.x * moveRate;
			yOffset = trans.y * moveRate;
			zOffset = -trans.z * moveRate;

            /*�悭�킩��Ȃ��@�v����*/  /*�͂����ĂȂ����ۂ�*/
                             /*kinect�J�����Ɛl�̈ʒu�֌W���Q�b�g���Ă�񂩂ȁH*/
            if (offsetRelativeToSensor)
            {
                //kinect�J�����̃|�W�V����
                Vector3 cameraPos = Camera.main.transform.position;

                float yRelToAvatar = (offsetNode != null ? offsetNode.transform.position.y : transform.position.y) - cameraPos.y;
                Vector3 relativePos = new Vector3(trans.x * moveRate, yRelToAvatar, trans.z * moveRate);
                Vector3 offsetPos = cameraPos + relativePos;

                if (offsetNode != null)
                {
                    offsetNode.transform.position = offsetPos;
                }
                else
                {
                    transform.position = offsetPos;
                }
            }
        }
	
        //���������C�������ȋC������  �������Ă���̂��낤��
        //trans�F�l�̈ʒu�@
		// Smoothly transition to the new position
		Vector3 targetPos = Kinect2AvatarPos(trans, verticalMovement);

        //�l�̈ʒu���������������œ������@smoothFactor����Ε␳���Ȃ��瓮����
        if (smoothFactor != 0f) {
            bodyRoot.localPosition = Vector3.Lerp(bodyRoot.localPosition, targetPos, smoothFactor * Time.deltaTime);
        }
        else
            bodyRoot.localPosition = targetPos;

        //bodyRoot��unity��ł̈ʒu�Ȃ̂��@kinect��ł̈ʒu�Ȃ̂��@���ׂĂ���  ����unity�ォ�ȁH
        /*trans�Fkinect��ԁ@�@targetPos bodyRoot�Funity��ԁ@�@�Ǝv���Ă���@�����Ă�H*/
	}
	

    //unity�����̃{�[���������ł���Ă�����ۂ�
	// If the bones to be mapped have been declared, map that bone to the model.
	protected virtual void MapBones()
	{
		// make OffsetNode as a parent of model transform.
		offsetNode = new GameObject(name + "Ctrl") { layer = transform.gameObject.layer, tag = transform.gameObject.tag };
		offsetNode.transform.position = transform.position;
		offsetNode.transform.rotation = transform.rotation;
		offsetNode.transform.parent = transform.parent;
		
		transform.parent = offsetNode.transform;
		transform.localPosition = Vector3.zero;
		transform.localRotation = Quaternion.identity;
		
		// take model transform as body root
		bodyRoot = transform;   //bodyRoot��unity���E���W
        
        //�L�����̃{�[�����Ƃ�
        // get bone transforms from the animator component
        var animatorComponent = GetComponent<Animator>();       //animatorComponen��unity�����̏��p�N���Ă���

        for (int boneIndex = 0; boneIndex < bones.Length; boneIndex++)
		{
			if (!boneIndex2MecanimMap.ContainsKey(boneIndex)) 
				continue;
            //�A�j���[�^�̒���GetBoneTransform�������Ă������{�[���ɓ���Ă���
			bones[boneIndex] = animatorComponent.GetBoneTransform(boneIndex2MecanimMap[boneIndex]);
		}
	}
	

    /*�{�[���̈ʒu����p�x���v�Z����  �����p*/
	// Capture the initial rotations of the bones
	protected void GetInitialRotations()
	{
		// save the initial rotation
		if(offsetNode != null)
		{
			initialPosition = offsetNode.transform.position;
			initialRotation = offsetNode.transform.rotation;
			
			offsetNode.transform.rotation = Quaternion.identity;
		}
		else
		{
			initialPosition = transform.position;
			initialRotation = transform.rotation;
			
			transform.rotation = Quaternion.identity;
		}
		
		for (int i = 0; i < bones.Length; i++)
		{
			if (bones[i] != null)
			{
				initialRotations[i] = bones[i].rotation; // * Quaternion.Inverse(initialRotation);
				initialLocalRotations[i] = bones[i].localRotation;
			}
		}
		
		// Restore the initial rotation
		if(offsetNode != null)
		{
			offsetNode.transform.rotation = initialRotation;
		}
		else
		{
			transform.rotation = initialRotation;
		}
	}
	





	// Converts kinect joint rotation to avatar joint rotation, depending on joint initial rotation and offset rotation
	protected Quaternion Kinect2AvatarRot(Quaternion jointRotation, int boneIndex)
	{
		// Apply the new rotation.
        Quaternion newRotation = jointRotation * initialRotations[boneIndex];
		
		//If an offset node is specified, combine the transform with its
		//orientation to essentially make the skeleton relative to the node
		if (offsetNode != null)
		{
			// Grab the total rotation by adding the Euler and offset's Euler.
			Vector3 totalRotation = newRotation.eulerAngles + offsetNode.transform.rotation.eulerAngles;
			// Grab our new rotation.
			newRotation = Quaternion.Euler(totalRotation);
		}
		
		return newRotation;
	}
	



    /*Pos��unity�ォ�@kinect�ォ�͂����肵�Ă���*/
    /*�������@�l�̈ړ��̃��C������*/
	// Converts Kinect position to avatar skeleton position, depending on initial position, mirroring and move rate
	protected Vector3 Kinect2AvatarPos(Vector3 jointPosition, bool bMoveVertically)
	{
        //jointPosition�F�L�l�N�g�摜��H�ł̐l�̈ʒu

        //��unity��?�Ł@�l�͂ǂ��ɂ���̂ł��傤���@�����߂Ă���?
        float xPos;
		float yPos;
		float zPos;

        //moveRate�����Ă��邩��unity��ԏ�H
        // If movement is mirrored, reverse it.
        if (!mirroredMovement)
            xPos = jointPosition.x * moveRate;// - xOffset;
        else 
        	xPos = -jointPosition.x * moveRate;// - xOffset;		
        yPos = jointPosition.y * moveRate - yOffset;
        zPos = -jointPosition.z * moveRate - zOffset;



        //�l?�̊֐߂̈ʒu��
        // If we are tracking vertical movement, update the y. Otherwise leave it alone.
        //     Vector3 avatarJointPos = new Vector3(xPos, bMoveVertically ? yPos : 0f, zPos);
        if (yPos < 0) yPos = 0;
        Vector3 avatarJointPos = new Vector3(xPos, yPos,0);
   //     Vector3 avatarJointPos = new Vector3(xPos, 0, 0);
        return avatarJointPos;
	}
	




    /*�������牺�ŃL�����̃{�[���̈ʒu�͂����Ă����?*/

	// dictionaries to speed up bones' processing
	// the author of the terrific idea for kinect-joints to mecanim-bones mapping
	// along with its initial implementation, including following dictionary is
	// Mikhail Korchun (korchoon@gmail.com). Big thanks to this guy!
	private readonly Dictionary<int, HumanBodyBones> boneIndex2MecanimMap = new Dictionary<int, HumanBodyBones>
	{
		{0, HumanBodyBones.Hips},
		{1, HumanBodyBones.Spine},
		{2, HumanBodyBones.Neck},
		{3, HumanBodyBones.Head},
		
		{4, HumanBodyBones.LeftShoulder},
		{5, HumanBodyBones.LeftUpperArm},
		{6, HumanBodyBones.LeftLowerArm},
		{7, HumanBodyBones.LeftHand},
		{8, HumanBodyBones.LeftIndexProximal},

		{9, HumanBodyBones.RightShoulder},
		{10, HumanBodyBones.RightUpperArm},
		{11, HumanBodyBones.RightLowerArm},
		{12, HumanBodyBones.RightHand},
		{13, HumanBodyBones.RightIndexProximal},

		{14, HumanBodyBones.LeftUpperLeg},
		{15, HumanBodyBones.LeftLowerLeg},
		{16, HumanBodyBones.LeftFoot},
		{17, HumanBodyBones.LeftToes},
		
		{18, HumanBodyBones.RightUpperLeg},
		{19, HumanBodyBones.RightLowerLeg},
		{20, HumanBodyBones.RightFoot},
		{21, HumanBodyBones.RightToes},
	};
	
    /*�����݂��ق����悢����*/
	protected readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> boneIndex2JointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
	{
		{0, KinectWrapper.NuiSkeletonPositionIndex.HipCenter},
		{1, KinectWrapper.NuiSkeletonPositionIndex.Spine},
		{2, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
		{3, KinectWrapper.NuiSkeletonPositionIndex.Head},
		
		{5, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
		{6, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
		{7, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
		{8, KinectWrapper.NuiSkeletonPositionIndex.HandLeft},
		
		{10, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
		{11, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
		{12, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
		{13, KinectWrapper.NuiSkeletonPositionIndex.HandRight},

        {14, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
        {15, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
        {16, KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft},
        {17, KinectWrapper.NuiSkeletonPositionIndex.FootLeft},

        {18, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
        {19, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
        {20, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
        {21, KinectWrapper.NuiSkeletonPositionIndex.FootRight},
    };
	
	protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2JointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
		{9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
	};
	
	protected readonly Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex> boneIndex2MirrorJointMap = new Dictionary<int, KinectWrapper.NuiSkeletonPositionIndex>
	{
		{0, KinectWrapper.NuiSkeletonPositionIndex.HipCenter},
		{1, KinectWrapper.NuiSkeletonPositionIndex.Spine},
		{2, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter},
		{3, KinectWrapper.NuiSkeletonPositionIndex.Head},
		
		{5, KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight},
		{6, KinectWrapper.NuiSkeletonPositionIndex.ElbowRight},
		{7, KinectWrapper.NuiSkeletonPositionIndex.WristRight},
		{8, KinectWrapper.NuiSkeletonPositionIndex.HandRight},
		
		{10, KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft},
		{11, KinectWrapper.NuiSkeletonPositionIndex.ElbowLeft},
		{12, KinectWrapper.NuiSkeletonPositionIndex.WristLeft},
		{13, KinectWrapper.NuiSkeletonPositionIndex.HandLeft},
		
		{14, KinectWrapper.NuiSkeletonPositionIndex.HipRight},
		{15, KinectWrapper.NuiSkeletonPositionIndex.KneeRight},
		{16, KinectWrapper.NuiSkeletonPositionIndex.AnkleRight},
		{17, KinectWrapper.NuiSkeletonPositionIndex.FootRight},
		
		{18, KinectWrapper.NuiSkeletonPositionIndex.HipLeft},
		{19, KinectWrapper.NuiSkeletonPositionIndex.KneeLeft},
		{20, KinectWrapper.NuiSkeletonPositionIndex.AnkleLeft},
		{21, KinectWrapper.NuiSkeletonPositionIndex.FootLeft},
	};
	
	protected readonly Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>> specIndex2MirrorJointMap = new Dictionary<int, List<KinectWrapper.NuiSkeletonPositionIndex>>
	{
		{4, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderRight, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
		{9, new List<KinectWrapper.NuiSkeletonPositionIndex> {KinectWrapper.NuiSkeletonPositionIndex.ShoulderLeft, KinectWrapper.NuiSkeletonPositionIndex.ShoulderCenter} },
	};
	
}

